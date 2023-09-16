using LSTY.Sdtd.Shared.Models;

namespace LSTY.Sdtd.Shared.Hubs
{
    /// <summary>
    /// 实体管理中心
    /// </summary>
    public interface IEntityManageHub
    {
        #region Player
        /// <summary>
        /// 获取指定实体Id的在线玩家
        /// </summary>
        /// <param name="entityId">实体Id</param>
        /// <returns></returns>
        Task<OnlinePlayer?> GetOnlinePlayer(int entityId);

        /// <summary>
        /// 通过玩家Id、实体Id或名称获取玩家基础信息
        /// </summary>
        /// <param name="idOrName">玩家Id或名称</param>
        /// <returns></returns>
        Task<PlayerBase?> GetPlayerByIdOrName(string idOrName);

        /// <summary>
        /// 获取在线玩家数量
        /// </summary>
        /// <returns></returns>
        Task<int> GetOnlinePlayerCount();

        /// <summary>
        /// 获取指定玩家的库存
        /// </summary>
        /// <param name="entityId">实体Id</param>
        /// <returns></returns>
        Task<Inventory?> GetPlayerInventory(int entityId);

        /// <summary>
        /// 获取所有玩家库存
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<int, Inventory>> GetPlayersInventory();

        /// <summary>
        /// 获取所有玩家
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<OnlinePlayer>> GetOnlinePlayers();

        /// <summary>
        /// 判断两个玩家是否为好友
        /// </summary>
        /// <param name="entityId">实体Id</param>
        /// <param name="anotherEntityId">另一个玩家实体Id</param>
        /// <returns></returns>
        Task<bool> IsFriend(int entityId, int anotherEntityId);

        /// <summary>
        /// 传送玩家
        /// </summary>
        /// <param name="originPlayerIdOrName">被传送玩家的Id或昵称</param>
        /// <param name="targetPlayerIdOrNameOrPosition">目标玩家的Id、名称或坐标</param>
        /// <returns></returns>
        Task<IEnumerable<string>> TeleportPlayer(string originPlayerIdOrName, string targetPlayerIdOrNameOrPosition);

        /// <summary>
        /// 通过实体Id获取玩家坐标
        /// </summary>
        /// <param name="entityId">实体Id</param>
        /// <returns></returns>
        Task<Position?> GetPlayerPosition(int entityId);

        /// <summary>
        /// 获取指定实体类型的位置
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<EntityInfo>> GetEntitiesLocation(EntityType entityType);
        #endregion

        #region Other

        /// <summary>
        /// 生成实体
        /// </summary>
        /// <param name="spawnEntityIdOrName">生成的实体Id或名称</param>
        /// <param name="targetPlayerIdOrName">目标玩家的Id或昵称</param>
        /// <returns></returns>
        Task<IEnumerable<string>> SpawnEntity(string spawnEntityIdOrName, string targetPlayerIdOrName);
        #endregion
    }
}