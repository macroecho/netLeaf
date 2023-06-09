using NetLeaf.Options;
using NetLeaf.Repositorys.Entitys;
using NetLeaf.Repositorys.MySql.Core;

namespace NetLeaf.Repositorys.MySql
{
    internal class LeafSegmentRepository : ILeafSegmentRepository
    {
        private readonly MySqlOptions _mySqlOptions;

        internal LeafSegmentRepository(MySqlOptions mySqlOptions)
        {
            _mySqlOptions = mySqlOptions;
        }

        LeafSegment ILeafSegmentRepository.UpdateMaxIdAndGet()
        {
            using var db = new DbAccess(_mySqlOptions);
            var transaction = db.BeginTransaction();

            try
            {
                db.ExecuteNonQuery("UPDATE leaf_segment SET max_id = max_id + step", transaction);
                // 查询数据。
                var dr = db.ExecuteReader("SELECT max_id, step, time FROM leaf_segment", transaction);

                LeafSegment? result = null;

                if (dr.Read())
                {
                    result = new LeafSegment
                    {
                        MaxId = dr.GetInt64(0),
                        Step = dr.GetInt32(1),
                        Time = dr.GetDateTime(2)
                    };
                }

                dr.Close();

                transaction.Commit();

                return result;
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        async Task<LeafSegment> ILeafSegmentRepository.UpdateMaxIdAndGetAsync()
        {
            using var db = new DbAccess(_mySqlOptions);
            var transaction = await db.BeginTransactionAsync();

            try
            {
                await db.ExecuteNonQueryAsync("UPDATE leaf_segment SET max_id = max_id + step", transaction);
                // 查询数据。
                var dr = await db.ExecuteReaderAsync("SELECT max_id, step, time FROM leaf_segment", transaction);

                LeafSegment? result = null;

                if (await dr.ReadAsync())
                {
                    result = new LeafSegment
                    {
                        MaxId = dr.GetInt64(0),
                        Step = dr.GetInt32(1),
                        Time = dr.GetDateTime(2)
                    };
                }

                dr.Close();

                await transaction.CommitAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        async Task<LeafSegment> ILeafSegmentRepository.UpdateMaxIdAndGetAsync(int step)
        {
            using var db = new DbAccess(_mySqlOptions);
            var transaction = await db.BeginTransactionAsync();

            try
            {
                db.ExecuteNonQuery($"UPDATE leaf_segment SET max_id = max_id + {step}");
                // 查询数据。
                var dr = await db.ExecuteReaderAsync("SELECT max_id, step, time FROM leaf_segment");

                LeafSegment? result = null;

                if (await dr.ReadAsync())
                {
                    result = new LeafSegment
                    {
                        MaxId = dr.GetInt64(0),
                        Step = dr.GetInt32(1),
                        Time = dr.GetDateTime(2)
                    };
                }

                await transaction.CommitAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }
    }
}
