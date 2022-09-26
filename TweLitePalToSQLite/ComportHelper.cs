// See https://aka.ms/new-console-template for more information
using System.IO.Ports;

public class ComportHelper : IDisposable
{
    public List<IComResultReceiver> receivers { get; set; } = new List<IComResultReceiver>();
    public string com { get; set; } = "";
    public System.IO.Ports.SerialPort? serial { get; set; } = null;
    public bool checkPort(string[] arguments)
    {
        var ports = System.IO.Ports.SerialPort.GetPortNames();
        if (ports == null) { return false; }
        if (arguments.Length == 0)
        {
            Console.WriteLine($"Please specify COM port {String.Join(", ", ports)}");
        }
        else
        {
            var fistMatchCom = arguments.Where(q => ports.Contains(q)).FirstOrDefault();
            if (fistMatchCom != null)
            {
                this.com = fistMatchCom;
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public void Open()
    {
        serial = new System.IO.Ports.SerialPort(com, 115200);
        serial.ReadTimeout = 100;//100msまで待機
        serial.DataReceived += Serial_DataReceived;
        serial.Open();
    }
    private void d(string msg)
    {
        //Console.WriteLine($"{DateTime.Now:MM/dd HH:mm:ss}\t{msg}");
    }

    private void Serial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
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
                    //parser.parse(line);
                    d(line);
                    foreach(var rec in receivers)
                    {
                        rec.receiveResult(line);
                    }
                    line = serial.ReadLine();
                }
                catch (TimeoutException te) {
                    line = null;
                }
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

    public void Dispose()
    {
        if (serial == null) { return; }
        try
        {
            if(serial.IsOpen) serial.Close();
        }
        catch (Exception e) { }
        try
        {
            serial.Dispose();
        }
        catch (Exception e) { }
    }
}