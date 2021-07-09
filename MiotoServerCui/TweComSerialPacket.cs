using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class TweComSerialPacket : PacketCommon, IMonoPacket
    {
        private Regex ptn = new Regex(@"\[([0-9a-f]{8}):([\d]{1,3})\] ([^*]{1,100})\*([0-9a-f]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string csv { get; internal set; } = "";

        public string key { get; private set; } = "";
        public byte seq { get; private set; } = 0;

        public List<float> values = new List<float>(); 

        public bool parse(string msg, ref int ofs)
        {
            int pos = ofs;

            //[81003542:148] $CT10,0,5,34,0,0*b1 の、「:」の位置をofsは返すので、
            //「[」の位置を取得し残りすべての文字列を作業用として確保する。
            int bracketOfs = -1;
            for(int i=10; i<11; i++)
            {
                if(pos - i < 0) { break; }
                if(msg[pos - i] == '[')
                {
                    bracketOfs = i;
                    break;
                }
            }
            if(bracketOfs==-1) { return false; }

            string packet = msg.Substring(pos - bracketOfs, msg.Length - (pos - bracketOfs));
            if (packet.Length == 0) { return false; }
            int posAst = packet.IndexOf('*');
            if(posAst + 3 > packet.Length) { return false; }
            packet = packet.Substring(0, posAst + 3);
            if(packet.Length==0) { return false; }

            var m = ptn.Match(packet);
            if (m.Success == false) { return false; }

            string frame = m.Groups[3].ToString() + "*";
            byte sum = 0;
            for (var i = 0; i < frame.Length; i++)
            {
                sum += (byte)frame[i];
            }
            sum = (byte)(0xFF - sum);
            sum += 1;
            var checksum = Convert.ToByte(m.Groups[4].ToString(),16);
            if(sum != checksum) { return false; }

            int updatePos = (pos - bracketOfs) + posAst + 3;
            ofs = updatePos;

            this.mac = Convert.ToUInt32(m.Groups[1].ToString(), 16);            
            this.csv = m.Groups[3].ToString();
            this.key = this.csv.Split(',')[0].Remove(0, 1).ToLower();// $ はずし
            this.csv = this.csv.Remove(0, this.csv.IndexOf(',')+1);//$CT10, の除去
            this.dt = DateTime.Now;
            this.seq = Convert.ToByte(m.Groups[2].ToString());


            if (key.CompareTo("ct10") == 0)
            {
                //float値の確保と、csv情報をdA(デシアンペア)->A(アンペア)化
                values.Clear();
                values.AddRange(this.csv.Split(',').Select(v => Convert.ToInt32(v) / 10.0f));
                var csvAry = values.Select(v => v.ToString("F1"));
                this.csv = string.Join(",", csvAry);

                if (MiotoServerWrapper.config != null)
                {
                    MiotoServerWrapper.config.updateSerialCurrentList(mac);
                    Ct10CtDetector.getInstance().fetch(this);
                    SerialPortWorker.memDbBackup(this.toCsv());
                }
            }


            return true;
        }

        public int insertCounter { get; private set; } = 0;

        public void registDb(DbWrapper db)
        {
            DbComSerial.getInstance().insert(this);

            insertCounter++;
            if (insertCounter > 100)
            {
                DbComSerial.getInstance().purgeBySec(MiotoServerWrapper.config.memoryDbPurgeSec);
                insertCounter = 0;
                Program.d("purge CT10 mem db.");
            }
        }



        public string toCsv(int preSeq)
        {
            int diffSeq = 0;
            if (seq >= preSeq)
            {
                diffSeq = seq - preSeq;
            }
            else
            {
                diffSeq = seq + (256 - preSeq);
            }

            return dt.ToString("yyyy/MM/dd HH:mm:ss") + string.Format(",{0:x},{1:D},", mac, diffSeq) + csv;
        }

        public string toCsv()
        {
            return this.toCsv(0);
        }

        public override string ToString()
        {
            return key + ") "+toCsv();
        }

    }
}
