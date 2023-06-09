using NetLeaf.Repositorys.Entitys;

namespace NetLeaf.Repositorys
{
    /// <summary>
    /// Leaf Id 号段仓储。
    /// </summary>
    internal interface ILeafSegmentRepository
    {
        internal LeafSegment UpdateMaxIdAndGet();

        /// <summary>
        /// 更新最大 Id 和获取号段信息。
        /// </summary>
        /// <returns> 号段信息。</returns>
        internal Task<LeafSegment> UpdateMaxIdAndGetAsync();

        internal Task<LeafSegment> UpdateMaxIdAndGetAsync(int step);
    }
}
