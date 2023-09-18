namespace LSTY.Sdtd.PatronsMod.Extensions
{
    internal static class ClientInfoExtension
    {
        public static PlayerBase ToPlayerBase(this ClientInfo clientInfo)
        {
            return new PlayerBase()
            {
                EntityId = clientInfo.entityId,
                CrossplatformId = clientInfo.CrossplatformId.CombinedString,
                Name = clientInfo.playerName,
                PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                PlatformId = clientInfo.PlatformId.CombinedString,
            };
        }

        public static SpawnedPlayer ToSpawnedPlayer(this ClientInfo clientInfo, RespawnType respawnType, Vector3i position)
        {
            return new SpawnedPlayer()
            {
                EntityId = clientInfo.entityId,
                CrossplatformId = clientInfo.CrossplatformId.CombinedString,
                Name = clientInfo.playerName,
                PlatformType = clientInfo.PlatformId.PlatformIdentifierString,
                PlatformId = clientInfo.PlatformId.CombinedString,
                RespawnType = (Shared.Models.RespawnType)respawnType,
                Position = position.ToPosition()
            };
        }
    }
}