using LSTY.Sdtd.PatronsMod.Extensions;
using Microsoft.AspNet.SignalR;

namespace LSTY.Sdtd.PatronsMod.Hubs
{
    public abstract class HubBase : Hub
    {
        protected static string FormatCommandArgs(string? args)
        {
            if(args == null)
            {
                return string.Empty;
            }

            if (args.Contains('\"'))
            {
                throw new Exception("参数不应该包含字符: '\"'");
            }

            if (args.Contains(' '))
            {
                return string.Concat("\"", args, "\"");
            }

            return args;
        }

        protected async Task<IEnumerable<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            if (inMainThread)
            {
                return await Task.Factory.StartNew((state) =>
                {
                    IEnumerable<string> executeResult = Enumerable.Empty<string>();
                    ModApi.MainThreadSyncContext.Send((innerState) =>
                    {
                        executeResult = SdtdConsole.Instance.ExecuteSync((string)innerState, Utils.CmdExecuteDelegate);
                    }, state);

                    return executeResult;
                }, command);
            }
            else
            {
                return await Task.Factory.StartNew((state) =>
                {
                    return SdtdConsole.Instance.ExecuteSync((string)state, Utils.CmdExecuteDelegate);
                }, command);
            }
        }
    }
}