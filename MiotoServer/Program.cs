using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiotoServerW
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] cmds = System.Environment.GetCommandLineArgs();
            OPT flg = OPT.NOMAL;
            //コマンドライン引数を列挙する
            foreach (string cmd in cmds)
            {
                Console.WriteLine(cmd);
                if (cmd.CompareTo("/i") == 0) { flg = OPT.INSTALL; }
                if (cmd.CompareTo("/u") == 0) { flg = OPT.UNINSTALL; }
            }
            switch (flg)
            {
                case OPT.INSTALL:
                    Installer.Install();
                    break;
                case OPT.UNINSTALL:
                    Installer.Uninstall();
                    break;
                case OPT.NOMAL:
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                    break;
            }


        }

        enum OPT { NOMAL, INSTALL, UNINSTALL };
    }
}
