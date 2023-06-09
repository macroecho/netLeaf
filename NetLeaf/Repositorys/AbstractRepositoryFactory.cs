namespace NetLeaf.Repositorys
{
    internal interface AbstractRepositoryFactory
    {
        internal abstract ILeafSegmentRepository CreateLeafSegmentRepository();
    }
}
