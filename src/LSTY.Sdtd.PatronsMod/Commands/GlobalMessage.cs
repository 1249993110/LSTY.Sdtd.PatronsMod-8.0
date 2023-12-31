﻿namespace LSTY.Sdtd.PatronsMod.Commands
{
    public class GlobalMessage : ConsoleCmdBase
    {
        protected override string getDescription()
        {
            return "Sends a message to all connected clients.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
               "  1. ty-gm <Message>\n" +
               "  2. ty-gm <Message> <SenderName>\n" +
               "1. Sends a message to all connected clients by default server name: " + Common.DefaultServerName + "\n" +
               "2. Sends a message to all connected clients by sender name";
        }

        protected override string[] getCommands()
        {
            return new string[]
            {
                "ty-GlobalMessage",
                "ty-gm",
                "ty-say"
            };
        }

        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            if (args.Count < 1)
            {
                Log("Wrong number of arguments, expected 1, found " + args.Count + ".");
                return;
            }

            string message = args[0];
            string senderName = (args.Count < 2 || string.IsNullOrEmpty(args[1])) ? Common.DefaultServerName : args[1];

            GameManager.Instance.ChatMessageServer(Utils.CmdExecuteDelegate, EChatType.Global, -1, message, senderName, false, null);
        }
    }
}