
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

using NetLeaf.Options;

namespace NetLeaf.Repositorys.MySql.Core
{
    internal class DbAccess : IDisposable
    {
        private static bool IsInit;
        private static object _lock = new object();

        private string _connectionString;
        private MySqlConnection _dbConnection;


        internal DbAccess(MySqlOptions mySqlOptions)
        {
            if (mySqlOptions.ConnectionString == null)
            {
                throw new ArgumentNullException(nameof(mySqlOptions.ConnectionString));
            }

            _connectionString = mySqlOptions.ConnectionString;

            if (!IsInit)
            {
                lock (_lock)
                {
                    if (!IsInit)
                    {
                        IsInit = true;

                        // 创建数据库、和数据库表。
                        CreateDatabase();
                    }
                }
            }
        }

        internal MySqlTransaction BeginTransaction()
        {
            var dbConnection = CreateConnection();

            dbConnection.Open();

            return dbConnection.BeginTransaction();
        }

        internal async Task<MySqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var dbConnection = CreateConnection();

            await dbConnection.OpenAsync(cancellationToken);

            return await dbConnection.BeginTransactionAsync(cancellationToken);
        }

        internal DbDataReader ExecuteReader(string sql)
        {
            var cmd = CreateCommand(new DbCmdParameter { Text = sql, Type = CommandType.Text });

            return cmd.ExecuteReader();
        }

        internal DbDataReader ExecuteReader(string sql, MySqlTransaction transaction)
        {
            var cmd = CreateCommand(new DbCmdParameter { Text = sql, Type = CommandType.Text, Transaction = transaction });

            return cmd.ExecuteReader();
        }

        internal int ExecuteNonQuery(string sql)
        {
            var cmd = CreateCommand(new DbCmdParameter { Text = sql, Type = CommandType.Text });

            return cmd.ExecuteNonQuery();
        }

        internal int ExecuteNonQuery(string sql, MySqlTransaction transaction)
        {
            var cmd = CreateCommand(new DbCmdParameter { Text = sql, Type = CommandType.Text, Transaction = transaction });

            return cmd.ExecuteNonQuery();
        }

        internal async Task<DbDataReader> ExecuteReaderAsync(string sql, CancellationToken cancellationToken = default)
        {
            var cmd = await CreateCommandAsync(new DbCmdParameter { Text = sql, Type = CommandType.Text }, cancellationToken);

            return await cmd.ExecuteReaderAsync(cancellationToken);
        }

        internal async Task<DbDataReader> ExecuteReaderAsync(string sql, MySqlTransaction transaction, CancellationToken cancellationToken = default)
        {
            var cmd = await CreateCommandAsync(new DbCmdParameter { Text = sql, Type = CommandType.Text, Transaction = transaction }, cancellationToken);

            return await cmd.ExecuteReaderAsync(cancellationToken);
        }

        internal async Task<int> ExecuteNonQueryAsync(string sql, MySqlTransaction transaction, CancellationToken cancellationToken = default)
        {
            var cmd = await CreateCommandAsync(new DbCmdParameter { Text = sql, Type = CommandType.Text, Transaction = transaction }, cancellationToken);

            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        internal void Close()
        {
            if (_dbConnection != null && _dbConnection.State != ConnectionState.Closed)
            {
                _dbConnection.Dispose();
            }
        }

        public void Dispose()
        {
            Close();

            _dbConnection = null;
        }

        private DbCommand CreateCommand(DbCmdParameter dbCommandParameter)
        {
            if (dbCommandParameter.Transaction == null)
            {
                _dbConnection = CreateConnection();
                _dbConnection.Open();
            }
            else
            {
                _dbConnection = dbCommandParameter.Transaction.Connection;
            }

            var cmd = new MySqlCommand();

            cmd.Connection = _dbConnection;
            cmd.CommandType = dbCommandParameter.Type;
            cmd.CommandText = dbCommandParameter.Text;

            if (dbCommandParameter.Transaction != null)
            {
                cmd.Transaction = dbCommandParameter.Transaction;
            }

            if (dbCommandParameter.Timeout != null)
            {
                cmd.CommandTimeout = dbCommandParameter.Timeout.Value;
            }

            if (dbCommandParameter.Parameters != null)
            {
                AddParameters(cmd.Parameters, dbCommandParameter.Parameters);
            }

            return cmd;
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        private async Task<DbCommand> CreateCommandAsync(DbCmdParameter dbCommandParameter, CancellationToken cancellationToken = default)
        {
            if (dbCommandParameter.Transaction == null)
            {
                _dbConnection = CreateConnection();
                await _dbConnection.OpenAsync(cancellationToken);
            }
            else
            {
                _dbConnection = dbCommandParameter.Transaction.Connection;
            }

            var cmd = new MySqlCommand();

            cmd.Connection = _dbConnection;
            cmd.CommandType = dbCommandParameter.Type;
            cmd.CommandText = dbCommandParameter.Text;

            if (dbCommandParameter.Transaction != null)
            {
                cmd.Transaction = dbCommandParameter.Transaction;
            }

            if (dbCommandParameter.Timeout != null)
            {
                cmd.CommandTimeout = dbCommandParameter.Timeout.Value;
            }

            if (dbCommandParameter.Parameters != null)
            {
                AddParameters(cmd.Parameters, dbCommandParameter.Parameters);
            }

            return cmd;
        }

        private void AddParameters(DbParameterCollection dbParameterCollection, IDictionary<string, object> parameters)
        {
            if (dbParameterCollection is MySqlParameterCollection parameterCollection)
            {
                foreach (var item in parameters)
                {
                    if (item.Value == null)
                    {
                        parameterCollection.AddWithValue(string.Concat("@", item.Key), DBNull.Value);
                    }
                    else
                    {
                        parameterCollection.AddWithValue(string.Concat("@", item.Key), item.Value);
                    }
                }
            }
        }

        private void CreateDatabase()
        {
            var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
            {
                // 更改为系统表。
                Database = "sys"
            };

            using var conn = new MySqlConnection(connectionBuilder.ConnectionString);

            conn.Open();

            using var cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = DbSeed.Script
            };

            cmd.ExecuteNonQuery();
        }

    }
}
