using NetLeaf.Options;
using NetLeaf.Repositorys;

namespace NetLeaf
{
    public class IdGeneratorFactory
    {
        private static IIdGenerator? _idGenerator;
        private static readonly object _lock = new();

        public static IIdGenerator GetDefault(string connectionString)
        {
            if (_idGenerator == null)
            {
                lock (_lock)
                {
                    _idGenerator ??= new LeafIdGenerator(new RepositoryFactoryManager(new RepositoryOptions
                    {
                        Type = RepositoryType.MySql,
                        MySqlOptions = new MySqlOptions
                        {
                            ConnectionString = connectionString
                        }
                    }).CreateLeafSegmentRepository());
                }
            }

            return _idGenerator;
        }

        public static IIdGenerator Get(RepositoryFactoryManager repositoryFactoryManager)
        {
            return new LeafIdGenerator(repositoryFactoryManager.CreateLeafSegmentRepository());
        }
    }
}
