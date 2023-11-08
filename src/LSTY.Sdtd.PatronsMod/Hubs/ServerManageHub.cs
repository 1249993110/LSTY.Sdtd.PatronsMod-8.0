using LSTY.Sdtd.PatronsMod.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System.Text;

namespace LSTY.Sdtd.PatronsMod
{
    [HubName(nameof(IServerManageHub))]
    public class ServerManageHub : Hubs.HubBase, IServerManageHub
    {
        public new async Task<IEnumerable<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            return await base.ExecuteConsoleCommand(command, inMainThread);
        }

        public async Task<IEnumerable<AllowedCommand>> GetAllowedCommands()
        {
            return await Task.Run(() =>
            {
                var result = new List<AllowedCommand>();
                foreach (var consoleCommand in SdtdConsole.Instance.GetCommands())
                {
                    var commands = consoleCommand.GetCommands();
                    int commandPermissionLevel = GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(commands);

                    result.Add(new AllowedCommand()
                    {
                        Commands = commands,
                        PermissionLevel = commandPermissionLevel,
                        Description = consoleCommand.GetDescription(),
                        Help = consoleCommand.GetHelp(),
                    });
                }

                return result;
            });
        }

        public async Task<IEnumerable<AllowSpawnedEntity>> GetAllowSpawnedEntities()
        {
            return await Task.Run(() =>
            {
                var result = new List<AllowSpawnedEntity>();
                int num = 1;
                foreach (var item in EntityClass.list.Dict.Values)
                {
                    if (item.userSpawnType == EntityClass.UserSpawnType.Menu)
                    {
                        result.Add(new AllowSpawnedEntity()
                        {
                            Id = num,
                            Name = item.entityClassName
                        });

                        ++num;
                    }
                }

                return result;
            });
        }

        public async Task<Shared.Models.GameStats> GetGameStats()
        {
            return await Task.Run(() =>
            {
                var worldTime = GameManager.Instance.World.GetWorldTime();
                var entityList = GameManager.Instance.World.Entities.list;

                int hostiles = 0;
                int animals = 0;
                foreach (var entity in entityList)
                {
                    if (entity.IsAlive())
                    {
                        if (entity is EntityEnemy)
                        {
                            ++hostiles;
                        }
                        else if (entity is EntityAnimal)
                        {
                            ++animals;
                        }
                    }
                }

                int onlinePlayers = GameManager.Instance.World.Players.Count;
                var persistentPlayers = GameManager.Instance.GetPersistentPlayerList()?.Players;
                int offlinePlayers = (persistentPlayers == null || persistentPlayers.Count == 0) ? 0 : persistentPlayers.Count - onlinePlayers;
                return new Shared.Models.GameStats()
                {
                    GameTime = new GameTime()
                    {
                        Days = GameUtils.WorldTimeToDays(worldTime),
                        Hours = GameUtils.WorldTimeToHours(worldTime),
                        Minutes = GameUtils.WorldTimeToMinutes(worldTime),
                    },
                    OnlinePlayers = onlinePlayers,
                    OfflinePlayers = offlinePlayers,
                    Hostiles = hostiles,
                    Animals = animals,
                };
            });
        }

        public async Task<ItemBlockPaged> GetItemBlocks(ItemBlockQuery itemBlockQuery)
        {
            return await Task.Factory.StartNew((state) =>
            {
                var param = (ItemBlockQuery)state;
                int pageSize = param.PageSize;
                List<ItemBlock> itemBlocks;
                string language = param.Language.ToString().ToLower();
                switch (param.ItemBlockKind)
                {
                    case ItemBlockKind.All:
                        itemBlocks = ItemsHelper.GetAllItemsAndBlocks(language, param.Keyword, param.ShowUserHidden);
                        break;

                    case ItemBlockKind.Item:
                        itemBlocks = ItemsHelper.GetAllItems(language, param.Keyword, param.ShowUserHidden);
                        break;

                    case ItemBlockKind.Block:
                        itemBlocks = ItemsHelper.GetAllBlocks(language, param.Keyword, param.ShowUserHidden);
                        break;
                    default:
                        itemBlocks = new List<ItemBlock>();
                        break;
                }

                var items = pageSize < 0 ? itemBlocks : itemBlocks.Skip((param.PageNumber - 1) * pageSize).Take(pageSize);

                var result = new ItemBlockPaged()
                {
                    Items = items,
                    Total = itemBlocks.Count
                };

                return result;
            }, itemBlockQuery);
        }

        public async Task<byte[]?> GetItemIcon(string iconName)
        {
            return await Task.Factory.StartNew((state) =>
            {
                string? iconPath = FindIconPath((string)state);
                if (iconPath == null)
                {
                    return null;
                }
                else
                {
                    return File.ReadAllBytes(iconPath);
                }
            }, iconName);
        }

        public async Task<IEnumerable<string>> GetKnownLanguages()
        {
            return await Task.Run(() =>
            {
                return Localization.dictionary["KEY"];
            });
        }

        public async Task<ClaimOwner?> GetLandClaim(string playerId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                var _playerId = (string)state;
                var claims = new List<Position>();
                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                var claimOwner = new ClaimOwner()
                {
                    ClaimPositions = claims
                };

                foreach (var item in persistentPlayerList.m_lpBlockMap)
                {
                    if (_playerId == item.Value.UserIdentifier.CombinedString)
                    {
                        var persistentPlayerData = item.Value;
                        claims.Add(item.Key.ToPosition());
                        claimOwner.PlatformId = persistentPlayerData.UserIdentifier.CombinedString;
                        claimOwner.PlayerName = persistentPlayerData.PlayerName;
                        bool claimActive = GameManager.Instance.World.IsLandProtectionValidForPlayer(persistentPlayerList.GetPlayerData(persistentPlayerData.UserIdentifier));
                        claimOwner.ClaimActive = claimActive;
                    }
                }

                return claimOwner;
            }, playerId);
        }

        public async Task<LandClaims> GetLandClaims()
        {
            return await Task.Run(() =>
            {
                int claimsize = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
                var claimOwners = new Dictionary<int, ClaimOwner>();

                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                if (persistentPlayerList == null)
                {
                    goto _Return;
                }

                var lpBlockMap = persistentPlayerList.m_lpBlockMap;
                if (lpBlockMap == null || lpBlockMap.Count == 0)
                {
                    goto _Return;
                }

                foreach (var item in lpBlockMap)
                {
                    var persistentPlayerData = item.Value;
                    int entityId = persistentPlayerData.EntityId;
                    if (claimOwners.ContainsKey(entityId))
                    {
                        ((List<Position>)claimOwners[entityId].ClaimPositions).Add(item.Key.ToPosition());
                    }
                    else
                    {
                        bool claimActive = GameManager.Instance.World.IsLandProtectionValidForPlayer(persistentPlayerList.GetPlayerData(persistentPlayerData.UserIdentifier));
                        claimOwners.Add(entityId, new ClaimOwner()
                        {
                            ClaimActive = claimActive,
                            PlatformId = persistentPlayerData.UserIdentifier.CombinedString,
                            PlayerName = persistentPlayerData.PlayerName,
                            ClaimPositions = new List<Position>() { item.Key.ToPosition() }
                        });
                    }
                }
            _Return:
                return new LandClaims()
                {
                    ClaimOwners = claimOwners.Values,
                    ClaimSize = claimsize
                };
            });
        }

        public async Task<Dictionary<string, string>> GetLocalization(Language language)
        {
            return await Task.Factory.StartNew((state) =>
            {
                string language = ((Language)state).ToString().ToLower();
                var dict = Localization.dictionary;
                int languageIndex = Array.LastIndexOf(dict["KEY"], language);

                if (languageIndex < 0)
                {
                    throw new Exception($"The specified language: {language} does not exist");
                }

                return dict.ToDictionary(p => p.Key, p => p.Value[languageIndex]);
            }, language);
        }

        public async Task<string?> GetLocalization(string itemName, Language language)
        {
            return await Task.Run(() =>
            {
                string _language = language.ToString().ToLower();
                if (string.IsNullOrEmpty(itemName))
                {
                    throw new ArgumentNullException(nameof(itemName));
                }

                var dict = Localization.dictionary;
                int languageIndex = Array.LastIndexOf(dict["KEY"], _language);

                if (languageIndex < 0)
                {
                    throw new Exception($"The specified language: {_language} does not exist");
                }

                if (dict.ContainsKey(itemName) == false)
                {
                    throw new Exception($"The specified itemName: {itemName} does not exist");
                }

                return dict[itemName][languageIndex];
            });
        }

        public async Task<IEnumerable<string>> GiveItem(GiveItem giveItemEntry)
        {
            return await ExecuteConsoleCommand($"ty-gi {FormatCommandArgs(giveItemEntry.TargetPlayerIdOrName)} {giveItemEntry.ItemName} {giveItemEntry.Count ?? 1} {giveItemEntry.Quality ?? 1} {giveItemEntry.Durability ?? 100}");
        }

        public async Task<IEnumerable<string>> RestartServer(bool force = false)
        {
            string cmd = "ty-rs";
            if (force)
            {
                cmd += " -f";
            }

            return await ExecuteConsoleCommand(cmd);
        }

        public async Task<IEnumerable<string>> SendGlobalMessage(GlobalMessage globalMessage)
        {
            return await ExecuteConsoleCommand($"ty-say {FormatCommandArgs(globalMessage.Message)} {FormatCommandArgs(globalMessage.SenderName) ?? Common.DefaultServerName}");
        }

        public async Task<IEnumerable<string>> SendPrivateMessage(PrivateMessage privateMessage)
        {
            return await ExecuteConsoleCommand($"ty-pm {FormatCommandArgs(privateMessage.TargetPlayerIdOrName)} {FormatCommandArgs(privateMessage.Message)} {FormatCommandArgs(privateMessage.SenderName) ?? Common.DefaultServerName}");
        }

        private static async Task<IEnumerable<string>> ExecuteConsoleCommandBatch<TObject>(IEnumerable<TObject> objects, Func<TObject, string> getCommand)
        {
            if (objects?.Any() == false)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            return await Task.Factory.StartNew((state) =>
            {
                var executeResult = new List<string>();
                var executeDelegate = Utils.CmdExecuteDelegate;
                foreach (TObject item in (IEnumerable<TObject>)state)
                {
                    string command = getCommand(item);
                    var resultEntry = SdtdConsole.Instance.ExecuteSync(command, executeDelegate);
                    executeResult.AddRange(resultEntry);
                }

                return executeResult;
            }, objects);
        }

        private static string? FindIconPath(string iconName)
        {
            string path = "Data/ItemIcons/" + iconName;
            if (File.Exists(path))
            {
                return path;
            }

            foreach (Mod mod in ModManager.GetLoadedMods())
            {
                var di = new DirectoryInfo(mod.Path);
                var files = di.GetFiles(iconName, SearchOption.AllDirectories);

                if(files.Length > 0)
                {
                    return files[0].FullName;
                }
            }

            return null;
        }

        #region Admins

        public async Task<IEnumerable<string>> AddAdmins(IEnumerable<AdminEntry> admins)
        {
            return await ExecuteConsoleCommandBatch(admins, obj => $"admin add {obj.PlayerId} {obj.PermissionLevel} {FormatCommandArgs(obj.DisplayName)}");
        }

        public async Task<IEnumerable<AdminEntry>> GetAdmins()
        {
            return await Task.Run(() =>
            {
                var result = new List<AdminEntry>();

                foreach (var item in GameManager.Instance.adminTools.Users.GetUsers().Values)
                {
                    result.Add(new AdminEntry()
                    {
                        PlayerId = item.UserIdentifier.CombinedString,
                        PermissionLevel = item.PermissionLevel,
                        DisplayName = item.Name,
                    });
                }

                return result;
            });
        }

        public async Task<IEnumerable<string>> RemoveAdmins(IEnumerable<string> playerId)
        {
            return await ExecuteConsoleCommandBatch(playerId, obj => $"admin remove {obj}");
        }

        #endregion Admins

        #region Permissions

        public async Task<IEnumerable<string>> AddPermissions(IEnumerable<PermissionEntry> permissions)
        {
            return await ExecuteConsoleCommandBatch(permissions, obj => $"cp add {obj.Command} {obj.Level}");
        }

        public async Task<IEnumerable<PermissionEntry>> GetPermissions()
        {
            return await Task.Run(() =>
            {
                var result = new List<PermissionEntry>();

                foreach (var item in GameManager.Instance.adminTools.Commands.GetCommands().Values)
                {
                    result.Add(new PermissionEntry()
                    {
                        Command = item.Command,
                        Level = item.PermissionLevel
                    });
                }

                return result;
            });
        }

        public async Task<IEnumerable<string>> RemovePermissions(IEnumerable<string> playerId)
        {
            return await ExecuteConsoleCommandBatch(playerId, obj => $"cp remove {obj}");
        }

        #endregion Permissions

        #region Whitelist

        public async Task<IEnumerable<string>> AddWhitelist(IEnumerable<WhitelistEntry> whitelist)
        {
            return await ExecuteConsoleCommandBatch(whitelist, obj => $"whitelist add {obj.PlayerId} {FormatCommandArgs(obj.DisplayName)}");
        }

        public async Task<IEnumerable<WhitelistEntry>> GetWhitelist()
        {
            return await Task.Run(() =>
            {
                var result = new List<WhitelistEntry>();

                foreach (var item in GameManager.Instance.adminTools.Whitelist.GetUsers().Values)
                {
                    result.Add(new WhitelistEntry()
                    {
                        PlayerId = item.UserIdentifier.CombinedString,
                        DisplayName = item.Name
                    });
                }

                return result;
            });
        }

        public async Task<IEnumerable<string>> RemoveWhitelist(IEnumerable<string> playerId)
        {
            return await ExecuteConsoleCommandBatch(playerId, obj => $"whitelist remove {obj}");
        }

        #endregion Whitelist

        #region Blacklist

        public async Task<IEnumerable<string>> AddBlacklist(IEnumerable<BlacklistEntry> blacklist)
        {
            return await ExecuteConsoleCommandBatch(blacklist, obj =>
            {
                return $"ban add {obj.PlayerId} {(int)(obj.BannedUntil - DateTime.Now).TotalMinutes} minutes {FormatCommandArgs(obj.Reason)} {FormatCommandArgs(obj.DisplayName)}";
            });
        }

        public async Task<IEnumerable<BlacklistEntry>> GetBlacklist()
        {
            return await Task.Run(() =>
            {
                var result = new List<BlacklistEntry>();

                foreach (var item in GameManager.Instance.adminTools.Blacklist.GetBanned())
                {
                    result.Add(new BlacklistEntry()
                    {
                        PlayerId = item.UserIdentifier.CombinedString,
                        BannedUntil = item.BannedUntil,
                        Reason = item.BanReason,
                        DisplayName = item.Name
                    });
                }

                return result;
            });
        }

        public async Task<IEnumerable<string>> RemoveBlacklist(IEnumerable<string> playerId)
        {
            return await ExecuteConsoleCommandBatch(playerId, obj => $"ban remove {obj}");
        }

        public Task<bool> IsBloodMoon()
        {
            return Task.FromResult(GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive);
        }

        #endregion Blacklist
    }
}