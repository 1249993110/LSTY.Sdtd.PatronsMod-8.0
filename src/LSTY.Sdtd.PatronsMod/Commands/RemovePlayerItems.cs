using LSTY.Sdtd.PatronsMod.Hooks;

namespace LSTY.Sdtd.PatronsMod.Commands
{
    public class RemovePlayerItems : ConsoleCmdBase
    {
        protected override string getDescription()
        {
            return "Removes an online player's items.";
        }

        protected override string getHelp()
        {
            return "Removes items with the specified name from a online player.\n" +
                "Usage: ty-rpi <EntityId/PlayerId/PlayerName> <ItemName>\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "ty-RemovePlayerItems", "ty-rpi" };
        }

        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (args.Count < 2)
                {
                    Log("Wrong number of arguments, expected 2, found " + args.Count + ".");
                    return;
                }

                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                if (cInfo != null)
                {
                    World world = GameManager.Instance.World;
                    var playersDict = world.Players.dict;

                    if (cInfo != null && playersDict.TryGetValue(cInfo.entityId, out EntityPlayer player))
                    {
                        string itemName = args[1];

                        // Remove the name of the block shape
                        int index = itemName.IndexOf(':');
                        if (index != -1)
                        {
                            itemName = itemName.Substring(0, index);
                        }

                        string actionName = WorldStaticDataHook.ActionPrefix + WorldStaticDataHook.TagPrefix + itemName;

                        if (GameEventManager.GameEventSequences.ContainsKey(actionName))
                        {
                            GameEventManager.Current.HandleAction(actionName, null, player, false, "");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>()
                                .Setup(actionName, cInfo.entityId, string.Empty, string.Empty, NetPackageGameEventResponse.ResponseTypes.Approved));

                            Log("Removed item: {0},action: {1} from inventory of player id '{2}' '{3}' named '{4}'",
                                itemName, actionName, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName);
                            return;
                        }
                        else
                        {
                            Log("Unable to locate {0} in the game events list", actionName);
                            return;
                        }
                    }
                }
                else
                {
                    Log("Unable to locate player '{0}' online", args[0]);
                    return;
                }
            }
            catch (Exception e)
            {
                Log("Error in RemoveItem.Execute: {0}", e.Message);
            }
        }
    }
}