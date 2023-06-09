using NetLeaf.Options;
using NetLeaf.Exceptions;
using NetLeaf.Repositorys.MySql;

namespace NetLeaf.Repositorys
{
    public class RepositoryFactoryManager
    {
        private readonly AbstractRepositoryFactory _abstractRepositoryFactory;

        public RepositoryFactoryManager(RepositoryOptions options)
        {
            if (options == null)
            {
                throw new OptionException(nameof(RepositoryOptions), "配置错误。");
            }

            _abstractRepositoryFactory = options.Type switch
            {
                RepositoryType.MySql => new MySqlRepositoryFactory(options.MySqlOptions),
                RepositoryType.SqlServer => throw new NotImplementedException(),
                RepositoryType.Oracle => throw new NotImplementedException(),
                RepositoryType.Mongodb => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        internal ILeafSegmentRepository CreateLeafSegmentRepository()
        {
            return _abstractRepositoryFactory.CreateLeafSegmentRepository();
        }
    }
}
