namespace NetLeaf.Repositorys.Entitys
{
    /// <summary>
    /// 存储 Leaf 生成 Id 的号段。
    /// </summary>
    internal class LeafSegment
    {
        /// <summary>
        /// 被分配的号段的最大值。
        /// </summary>
        internal long MaxId { get; set; }

        /// <summary>
        /// 每次分配的号段长度。
        /// 初始是 0~1000 的号段，当这个号段用完时，MaxId 会从 1000 被更新成 2000（MaxId = MaxId + Step）。
        /// 该值的大小决定更新 MaxId 的频率。
        /// </summary>
        internal int Step { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        internal DateTime Time { get; set; }
    }
}
