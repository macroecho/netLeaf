using NetLeaf.Options;
using NetLeaf.Exceptions;

namespace NetLeaf.Repositorys.MySql
{
    internal class MySqlRepositoryFactory : AbstractRepositoryFactory
    {
        private readonly MySqlOptions _mySqlOptions;

        internal MySqlRepositoryFactory(MySqlOptions mySqlOptions)
        {
            _mySqlOptions = mySqlOptions ?? throw new OptionException(nameof(MySqlOptions), "配置错误。");
        }

        ILeafSegmentRepository AbstractRepositoryFactory.CreateLeafSegmentRepository()
        {
            return new LeafSegmentRepository(_mySqlOptions);
        }
    }
}
