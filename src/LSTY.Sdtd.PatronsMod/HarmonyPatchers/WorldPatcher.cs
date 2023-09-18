using HarmonyLib;
using LSTY.Sdtd.PatronsMod.Extensions;
using LSTY.Sdtd.PatronsMod.Hubs;

namespace LSTY.Sdtd.PatronsMod.HarmonyPatchers
{
    [HarmonyPatch]
    public static class WorldPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(World), nameof(World.SpawnEntityInWorld))]
        public static void OnAfterSpawnEntityInWorld(Entity _entity)
        {
            if (_entity is EntityAlive entityAlive)
            {
                ModEventHook.EntitySpawned(new EntityInfo()
                {
                    EntityId = entityAlive.entityId,
                    EntityName = entityAlive.EntityName,
                    Position = entityAlive.position.ToPosition(),
                    EntityType = (LSTY.Sdtd.Shared.Models.EntityType)entityAlive.entityType
                });
            }
        }
    }
}