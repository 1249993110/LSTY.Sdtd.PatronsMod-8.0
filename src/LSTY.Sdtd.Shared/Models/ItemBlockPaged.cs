namespace LSTY.Sdtd.Shared.Models
{
    /// <summary>
    /// 物品方块分页
    /// </summary>
    public class ItemBlockPaged
    {
        /// <summary>
        /// 项目
        /// </summary>
        public IEnumerable<ItemBlock> Items { get; set; }

        /// <summary>
        /// 总计
        /// </summary>
        public int Total { get; set; }
    }
}