namespace NetLeaf
{
    /// <summary>
    /// 分布式唯一 Id 生成器。 
    /// </summary>
    public interface IIdGenerator
    {
        public void Init();

        /// <summary>
        /// 生成 Id。
        /// </summary>
        /// <returns> 唯一 Id。</returns>
        /// <exception cref="Exception"> ID 生成失败异常。</exception>
        public long Generation();

        public long Generation(out long previouId);
    }
}
