using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalcHelper
{
    class Installer
    {
        const string RULE_NAME = "CalcHelper service";
        const string EXE_NAME = "CalcHelper.exe";
        public static void Install()
        {
            //Firewall 
            runDosCmd("netsh advfirewall firewall add rule "
                + "name=\"" + RULE_NAME + "\" "
                + "dir=in action=allow protocol=TCP localport=80 "
                + "description=\"" + RULE_NAME + "用に外部接続を許可する\"");

            //タスクの登録
            var taskcmd = "schtasks /create /tn \"" + RULE_NAME + "\" "
                + "/tr \"'" + getInstallDir() + "\\" + EXE_NAME + "'\" /sc onlogon /rl highest /F";

            runDosCmd(taskcmd);
        }

        public static void Uninstall()
        {
            runDosCmd("netsh advfirewall firewall set rule name=\"" + RULE_NAME + "\" new enable=no");
            runDosCmd("netsh advfirewall firewall del rule name=\"" + RULE_NAME + "\" ");

            runDosCmd("schtasks /Delete /tn \"" + RULE_NAME + "\"  /F");
        }

        private static void runDosCmd(string arg, bool isOpenWindow = false)
        {
            var p = new Process();

            p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");//cmd.exeのパス取得
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.Arguments = "/c " + arg;//先頭の/cは終了後に閉じる処理

            if (!isOpenWindow) { p.StartInfo.CreateNoWindow = true; }
            p.Start();
            p.WaitForExit();
            p.Close();
        }

        private static string getInstallDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
