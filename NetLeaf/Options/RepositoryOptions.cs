using NetLeaf.Repositorys;

namespace NetLeaf.Options
{
    public class RepositoryOptions
    {
        public RepositoryType Type { get; set; }

        public MySqlOptions MySqlOptions { get; set; }
    }
}
