using System.Data;
using MySql.Data.MySqlClient;

namespace NetLeaf.Repositorys.MySql.Core
{
    internal class DbCmdParameter
    {
        internal CommandType Type { get; set; }

        internal Dictionary<string, object>? Parameters { get; set; }

        internal string? Text { get; set; }

        internal int? Timeout { get; set; }

        internal MySqlTransaction? Transaction { get; set; }
    }
}
