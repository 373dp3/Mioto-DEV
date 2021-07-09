using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TaskScheduler;


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
            RegistTaskScheduler();

        }

        private static void RegistTaskScheduler()
        {
            ITaskService taskservice = null;
            ITaskFolder rootfolder = null;
            try
            {
                // TaskServiceを生成して接続する
                // 接続時の User, Domain, Password を必要に応じて設定する
                taskservice = new TaskScheduler.TaskScheduler();
                taskservice.Connect(null, null, null, null);

                // タスクスケジューラーのフォルダを指定する
                rootfolder = taskservice.GetFolder("\\");
                var path = @"\" + RULE_NAME;

                // 新規登録用のタスクを定義
                ITaskDefinition taskDefinition = taskservice.NewTask(0);

                // 設定に使うもろもろ
                IRegistrationInfo registrationInfo = taskDefinition.RegistrationInfo;
                IActionCollection actionCollection = taskDefinition.Actions;
                IExecAction execAction = (IExecAction)actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                ITriggerCollection triggerCollection = taskDefinition.Triggers;
                ILogonTrigger logonTrigger = (ILogonTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                ITaskSettings taskSettings = taskDefinition.Settings;
                IPrincipal principal = taskDefinition.Principal;

                // タスクの作成者と概要
                registrationInfo.Author = "dp3 Toshiaki MINAMI";
                registrationInfo.Description = RULE_NAME + " installer にて設定されたタスクです。";

                // タスクの実行時に使うユーザアカウント
                principal.UserId = $@"{Environment.UserDomainName}\{Environment.UserName}";

                // ユーザがログオンしているかどうかにかかわらず実行
                // 最上位特権で実行する
                //principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_S4U;
                principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
                principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

                // トリガー条件(開始時間、間隔、有効)
                logonTrigger.StartBoundary = "2018-09-17T13:00:00";
                logonTrigger.Enabled = true;

                //タスクが3日以上経過しても終了させない
                taskSettings.ExecutionTimeLimit = "PT0S";

                //最上位優先度
                taskSettings.Priority = 7;

                // 実行ファイルの設定
                execAction.Path = $@"{AppDomain.CurrentDomain.BaseDirectory}" + EXE_NAME;

                // 作業ディレクトリをexeのあるパスに
                execAction.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 登録
                rootfolder.RegisterTaskDefinition(
                    path,
                    taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null,
                    null,
                    _TASK_LOGON_TYPE.TASK_LOGON_NONE,
                    null
                );
            }
            finally
            {
                if (taskservice != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskservice);
                }
                if (rootfolder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rootfolder);
                }
            }
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
