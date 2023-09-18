using LSTY.Sdtd.Shared.Models;

namespace LSTY.Sdtd.Shared.Hubs
{
    /// <summary>
    /// Mod Event Hub
    /// </summary>
    public interface IModEventHub
    {
        /// <summary>
        /// 游戏唤醒时触发
        /// </summary>
        void OnGameAwake();

        /// <summary>
        /// 游戏启动完成时触发
        /// </summary>
        void OnGameStartDone();

        /// <summary>
        /// 游戏关闭时触发
        /// </summary>
        void OnGameShutdown();

        /// <summary>
        /// 日志回调时触发
        /// </summary>
        /// <param name="logEntry">日志条目</param>
        void OnLogCallback(LogEntry logEntry);

        /// <summary>
        /// 玩家断开连接时触发
        /// </summary>
        /// <param name="entityId">实体Id</param>
        void OnPlayerDisconnected(int entityId);

        /// <summary>
        /// 玩家在世界中生成时触发
        /// </summary>
        /// <param name="spawnedPlayer"></param>
        void OnPlayerSpawnedInWorld(SpawnedPlayer spawnedPlayer);

        /// <summary>
        /// 玩家首次生成时触发
        /// </summary>
        /// <param name="playerBase">玩家基础信息</param>
        void OnPlayerSpawning(PlayerBase playerBase);

        /// <summary>
        /// 保存玩家数据时触发
        /// </summary>
        /// <param name="playerBase">玩家基础信息</param>
        void OnSavePlayerData(PlayerBase playerBase);

        /// <summary>
        /// 捕获聊天信息时触发
        /// </summary>
        /// <param name="chatMessage"></param>
        void OnChatMessage(ChatMessage chatMessage);

        /// <summary>
        /// 实体被击杀时触发
        /// </summary>
        /// <param name="killedEntity"></param>
        void OnEntityKilled(KilledEntity killedEntity);

        /// <summary>
        /// 实体生成时触发
        /// </summary>
        /// <param name="entityInfo"></param>
        void OnEntitySpawned(EntityInfo entityInfo);

        /// <summary>
        /// 天空改变时触发
        /// </summary>
        /// <param name="skyChanged"></param>
        void OnSkyChanged(SkyChanged skyChanged);

    }
}