using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LSTY.Sdtd.PatronsMod.Commands
{
    public class RestartServer : ConsoleCmdBase
    {
        protected override string getDescription()
        {
            return "Restart server, optional parameter -f";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-rs" +
                "  2. ty-rs -f" +
                "1. Restart server by shutdown" +
                "2. Force restart server";
        }

        protected override string[] getCommands()
        {
            return new[] { "ty-rs", "ty-RestartServer" };
        }

        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            Log("Server is restarting..., please wait");

            if (args.Count > 0)
            {
                if (string.Equals(args[0], "-f", StringComparison.OrdinalIgnoreCase))
                {
                    PrepareRestart(true);
                    return;
                }
            }

            PrepareRestart(false);
        }

        private static volatile bool _isRestarting;

        private void PrepareRestart(bool force = false)
        {
            SdtdConsole.Instance.ExecuteSync("sa", Utils.CmdExecuteDelegate);

            _isRestarting = true;

            if (force)
            {
                Restart();
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync("shutdown", Utils.CmdExecuteDelegate);
            }
        }

        private static void Restart()
        {
            string? scriptName = null;
            string? serverPath = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                scriptName = "restart-windows.bat";
                serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startdedicated.bat");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                scriptName = "restart-linux.sh";
                serverPath = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start("chmod", " +x " + Path.Combine(ModApi.ModDirectory, scriptName));
            }

            string path = Path.Combine(ModApi.ModDirectory, scriptName);
            Process.Start(path, string.Format("{0} \"{1}\"", Process.GetCurrentProcess().Id, serverPath));
        }

        public static void GameShutdown()
        {
            if (_isRestarting)
            {
                Restart();
            }
        }
    }
}