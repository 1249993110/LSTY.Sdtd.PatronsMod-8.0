namespace LSTY.Sdtd.PatronsMod.Extensions
{
    internal static class EntityPlayerExtension
    {
        public static OnlinePlayer? ToOnlinePlayer(this EntityPlayer player)
        {
            try
            {
                var clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
                var progression = player.Progression;
                var landProtection = GetLandProtectionActiveAndMultiplier(clientInfo.entityId);

                return new OnlinePlayer()
                {
                    CurrentLife = player.currentLife,
                    Deaths = player.Died,
                    EntityId = player.entityId,
                    CrossplatformId = clientInfo.CrossplatformId.CombinedString,
                    ExpToNextLevel = progression == null ? -1 : progression.ExpToNextLevel,
                    Health = player.Health,
                    IP = clientInfo.ip,
                    LandProtectionActive = landProtection.Item1,
                    LandProtectionMultiplier = landProtection.Item2,
                    Position = player.GetPosition().ToPosition(),
                    Level = progression == null ? -1 : progression.Level,
                    Name = player.EntityName,
                    Ping = clientInfo.ping,
                    PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                    PlatformId = clientInfo.PlatformId.CombinedString,
                    KilledPlayers = player.KilledPlayers,
                    Score = player.Score,
                    Stamina = player.Stamina,
                    TotalTimePlayed = player.totalTimePlayed,
                    KilledZombies = player.KilledZombies
                };
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "EntityPlayer to OnlinePlayer failed");
                return null;
            }
        }

        private static Tuple<bool, float> GetLandProtectionActiveAndMultiplier(int entityId)
        {
            try
            {
                var world = GameManager.Instance.World;
                var playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(entityId);

                return new Tuple<bool, float>(world.IsLandProtectionValidForPlayer(playerData),
                    world.GetLandProtectionHardnessModifierForPlayer(playerData));
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Get player land protection state failed");
                return new Tuple<bool, float>(false, -1);
            }
        }
    }
}