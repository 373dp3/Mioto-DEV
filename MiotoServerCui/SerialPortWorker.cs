using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class SerialPortWorker
    {
        public bool run(CancellationToken token)
        {
            var configPortList = MiotoServerWrapper.config.listComPort;
            var comPortList = new List<SerialPort>();

            //Open
            foreach(var port in configPortList)
            {
                if (port.portName.CompareTo(AppConfig.PORT_NO_USE_KEY) == 0) { continue; }
                var serial = new SerialPort(port.portName, Convert.ToInt32(port.portBps));
                comPortList.Add(serial);
#if MONO
                serial.NewLine = "\r\n";
#else
                serial.DataReceived += Serial_DataReceived;
#endif
                serial.ReadTimeout = 100;//100msまで待機
                serial.Open();
            }

            //終了待機
#if MONO
            while (token.IsCancellationRequested == false)
            {
                foreach(var port in comPortList)
                {
                    if(port.BytesToRead == 0) { continue; }
                    monoSerialReceived(port);
                }
                Thread.Sleep(50);
            }
#else
            while (token.IsCancellationRequested == false)
            {
                Thread.Sleep(1000);
            }
#endif

            //Close
            if (backupFileInfo!=null) { backupFileInfo.Dispose(); }
            foreach (var port in comPortList)
            {
                try
                {
                    port.Close();
                }
                catch(ObjectDisposedException ode) { }
                catch(Exception e)
                {
                    d(e.ToString());
                }
            }
            d("シリアルポート処理を終了しました。");
            return true;
        }

        class BackupFile : IDisposable
        {
            public StreamWriter sw { get; set; }
            public DateTime dtLimit { get; set; }

            public static BackupFile getBackupFile()
            {
                var ans = new BackupFile();

                var dt = DateTime.Now;
                var filePath = MiotoServerWrapper.config.dbdir
                                + Path.DirectorySeparatorChar
                                + String.Format("{0:ddd}-{1:HH}.csv", dt, dt);
                var fi = new FileInfo(filePath);

                //5日以上、以前の日付に作ったファイルの場合、削除する。
                if (File.Exists(filePath) && ((dt - fi.CreationTime).TotalDays > 5)) { File.Delete(filePath); }

                ans.sw = new StreamWriter(filePath, true);
                dt = dt.AddHours(1);//1hまで
                ans.dtLimit = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0);//端数切捨て

                return ans;
            }
            public bool isExpired()
            {
                if (DateTime.Now > dtLimit) return true;
                return false;
            }

            public void Dispose()
            {
                try
                {
                    sw.Close();
                    sw.Dispose();
                }
                catch (Exception e) { }
                sw = null;
            }
        }
        private static BackupFile backupFileInfo = null;
        public static void memDbBackup(string msg)
        {
            try
            {
                if (MiotoServerWrapper.config == null)
                {
                    d("MiotoServerWrapper.configがnullです。");
                }
                if (!MiotoServerWrapper.config.isMemoryDbBackup) { return; }
            }
            catch (Exception e)
            {
                d("Exception:" + e.ToString());
                return;
            }

            if ((backupFileInfo == null) || (backupFileInfo.sw == null))
            {
                try
                {
                    backupFileInfo = BackupFile.getBackupFile();
                }
                catch (Exception ee)
                {
                    d("exception 2 :" +ee.ToString());
                    return;
                }

            }
            if (backupFileInfo.isExpired())
            {
                try
                {
                    backupFileInfo.Dispose();
                }
                catch (Exception e3)
                {
                    d("e3 :"  + e3.ToString());
               
                }
                try
                {
                    backupFileInfo = BackupFile.getBackupFile();
                }
                catch (Exception e4)
                {
                    d("e4:" + e4.ToString());
                }
            }

            //1hごとにファイルを更新。曜日別で同じファイルが有る場合は削除
            if(backupFileInfo.sw == null)
            {
                d("sw is null");
                return;
            }
            backupFileInfo.sw.WriteLine(msg);
        }

        private static void d(string msg)
        {
            MiotoServerWrapper.d(msg);
        }

        static TwePacketParser parser = new TwePacketParser();

        private static void monoSerialReceived(SerialPort port)
        {
            try
            {
                var line = port.ReadLine();
                if (line.Length == 0) { return; }
                parser.parse(line);
            }
            catch (TimeoutException te) { }
            catch (System.IndexOutOfRangeException oor) { }
            catch (Exception e)
            {
                d(e.ToString());
            }
            finally { }
        }

        private static void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!(sender is SerialPort)) { return; }
            var serial = (SerialPort)sender;
            try
            {
                string line = serial.ReadLine();
                while (line != null)
                {
                    try
                    {
                        parser.parse(line);
                        line = serial.ReadLine();
                    }
                    catch (TimeoutException te) { line = null; }
                    catch (IndexOutOfRangeException ie) { line = null; }
                    catch (FormatException fe) { line = null; }
                    catch (System.InvalidOperationException ine) { line = null; }
                    catch (System.IO.IOException ioe) { line = null; }
                    
                    catch (Exception ex)
                    {
                        d("ex: " + ex.ToString());
                        line = null;
                    }
                }
            }
            catch (Exception ee) { }
            finally { }
        }
    }
}
