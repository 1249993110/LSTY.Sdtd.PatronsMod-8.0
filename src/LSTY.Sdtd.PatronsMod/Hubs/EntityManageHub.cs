using LSTY.Sdtd.PatronsMod.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using static XUiC_SpawnEntitiesList;

namespace LSTY.Sdtd.PatronsMod
{
    [HubName(nameof(IEntityManageHub))]
    public class EntityManageHub : Hubs.HubBase, IEntityManageHub
    {
        public async Task<OnlinePlayer?> GetOnlinePlayer(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                if (GameManager.Instance.World.Players.dict.TryGetValue((int)state, out var player))
                {
                    return player.ToOnlinePlayer();
                }

                return null;
            }, entityId);
        }

        public async Task<IEnumerable<OnlinePlayer>> GetOnlinePlayers()
        {
            return await Task.Run(() =>
            {
                var result = new List<OnlinePlayer>();
                foreach (var player in GameManager.Instance.World.Players.dict.Values)
                {
                    var onlinePlayer = player.ToOnlinePlayer();
                    if (onlinePlayer != null)
                    {
                        result.Add(onlinePlayer);
                    }
                }

                return result;
            });
        }

        public async Task<PlayerBase?> GetPlayerByIdOrName(string idOrName)
        {
            return await Task.Factory.StartNew((state) => ConsoleHelper.ParseParamIdOrName((string)state)?.ToPlayerBase(), idOrName);
        }

        public async Task<int> GetOnlinePlayerCount()
        {
            return await Task.FromResult(GameManager.Instance.World.Players.Count);
        }

        public async Task<Shared.Models.Inventory?> GetPlayerInventory(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                return ConnectionManager.Instance.Clients.ForEntityId((int)state)?.latestPlayerData.GetInventory();
            }, entityId);
        }

        public async Task<Dictionary<int, Shared.Models.Inventory>> GetPlayersInventory()
        {
            return await Task.Run(() =>
            {
                var result = new Dictionary<int, Shared.Models.Inventory>();
                foreach (var player in GameManager.Instance.World.Players.dict.Values)
                {
                    var inventory = ConnectionManager.Instance.Clients.ForEntityId(player.entityId)?.latestPlayerData.GetInventory();
                    if (inventory != null)
                    {
                        result.Add(player.entityId, inventory);
                    }
                }

                return result;
            });
        }

        public async Task<bool> IsFriend(int entityId, int anotherEntityId)
        {
            return await Task.Run(() =>
            {
                var players = GameManager.Instance.World.Players.dict;
                if (players.TryGetValue(entityId, out var entityPlayer) == false)
                {
                    return false;
                }

                if (players.TryGetValue(anotherEntityId, out var anotherEntityPlayer) == false)
                {
                    return false;
                }

                return entityPlayer.IsFriendsWith(anotherEntityPlayer);
            });
        }

        public async Task<IEnumerable<string>> TeleportPlayer(string originPlayerIdOrName, string targetPlayerIdOrNameOrPosition)
        {
            string target = targetPlayerIdOrNameOrPosition;
            if (target.Split(' ').Length != 3)
            {
                target = FormatCommandArgs(target);
            }

            return await ExecuteConsoleCommand($"tele {FormatCommandArgs(originPlayerIdOrName)} {target}");
        }
        
        public async Task<Position?> GetPlayerPosition(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                var players = GameManager.Instance.World.Players.dict;
                if (players.TryGetValue((int)state, out var entityPlayer) == false)
                {
                    return null;
                }

                return entityPlayer.position.ToPosition();
            }, entityId);
        }
        
        public async Task<IEnumerable<EntityInfo>> GetEntitiesLocation(Shared.Models.EntityType entityType)
        {
            return await Task.Factory.StartNew((state) =>
            {
                var _entityType = (Shared.Models.EntityType)state;
                var location = new List<EntityInfo>();

                if (_entityType == Shared.Models.EntityType.Player)
                {
                    foreach (var players in GameManager.Instance.World.Players.dict.Values)
                    {
                        location.Add(new EntityInfo()
                        {
                            EntityId = players.entityId,
                            EntityName = players.EntityName,
                            Position = players.GetPosition().ToPosition(),
                            EntityType = Shared.Models.EntityType.Player,
                        });
                    }
                }
                else if(_entityType == Shared.Models.EntityType.Animal) 
                {
                    foreach (var entity in GameManager.Instance.World.Entities.list)
                    {
                        if (entity is EntityAnimal entityAnimal && entity.IsAlive())
                        {
                            location.Add(new EntityInfo()
                            {
                                EntityId = entityAnimal.entityId,
                                EntityName = entityAnimal.EntityName ?? ("animal class #" + entityAnimal.entityClass),
                                Position = entityAnimal.GetPosition().ToPosition(),
                                EntityType = Shared.Models.EntityType.Animal,
                            });
                        }
                    }
                }
                else if (_entityType == Shared.Models.EntityType.Hostiles)
                {
                    foreach (var entity in GameManager.Instance.World.Entities.list)
                    {
                        if (entity is EntityEnemy entityEnemy && entity.IsAlive())
                        {
                            location.Add(new EntityInfo()
                            {
                                EntityId = entityEnemy.entityId,
                                EntityName = entityEnemy.EntityName ?? ("enemy class #" + entityEnemy.entityClass),
                                Position = entityEnemy.GetPosition().ToPosition(),
                                EntityType = (LSTY.Sdtd.Shared.Models.EntityType)entityEnemy.entityType
                            });
                        }
                    }
                }
                else if (_entityType == Shared.Models.EntityType.Zombie)
                {
                    foreach (var entity in GameManager.Instance.World.Entities.list)
                    {
                        if (entity is EntityZombie entityZombie && entity.IsAlive())
                        {
                            location.Add(new EntityInfo()
                            {
                                EntityId = entityZombie.entityId,
                                EntityName = entityZombie.EntityName ?? ("zombie class #" + entityZombie.entityClass),
                                Position = entityZombie.GetPosition().ToPosition(),
                                EntityType = (LSTY.Sdtd.Shared.Models.EntityType)entityZombie.entityType
                            });
                        }
                    }
                }
                else if (_entityType == Shared.Models.EntityType.Bandit)
                {
                    foreach (var entity in GameManager.Instance.World.Entities.list)
                    {
                        if (entity is EntityBandit entityBandit && entity.IsAlive())
                        {
                            location.Add(new EntityInfo()
                            {
                                EntityId = entityBandit.entityId,
                                EntityName = entityBandit.EntityName ?? ("bandit class #" + entityBandit.entityClass),
                                Position = entityBandit.GetPosition().ToPosition(),
                                EntityType = (LSTY.Sdtd.Shared.Models.EntityType)entityBandit.entityType
                            });
                        }
                    }
                }

                return location;
            }, entityType);
        }

        public async Task<IEnumerable<string>> SpawnEntity(string spawnEntityIdOrName, string targetPlayerIdOrName)
        {
            return await ExecuteConsoleCommand($"se {FormatCommandArgs(targetPlayerIdOrName)} {FormatCommandArgs(spawnEntityIdOrName)}");
        }
    }
}