using System.Diagnostics;
using System.Collections.Concurrent;

using Xunit.Abstractions;

namespace NetLeaf.Tests
{
    public class IdGeneratorTest
    {
        private const string ConnectionString = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=keikei;DataBase=netleaf;CharSet=utf8;allow zero datetime=true;Max Pool Size=10000;";

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IIdGenerator _idGenerator;

        public IdGeneratorTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _idGenerator = IdGeneratorFactory.GetDefault(ConnectionString);
            _idGenerator.Init();
        }

        [Fact]
        public void Generation()
        {
            var list = new ConcurrentBag<long>();
            var tasks = new List<Task>();

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            for (int i = 0, length = 100000; i < length; i++)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        list.Add(_idGenerator.Generation());
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            stopwatch.Stop();

            _testOutputHelper.WriteLine($"消耗的时间：{stopwatch.ElapsedMilliseconds}");

            var distinct = list.Distinct();
            var result = list.Count == distinct.Count();

            _testOutputHelper.WriteLine
            (
                $"result: {result}, list count: {list.Count}, distinct count: {distinct.Count()}, minId: {list.Min()}, maxId: {list.Max()}, differ: {list.Max() - list.Min()}"
            );

            Assert.Equal(list.Count, distinct.Count());
        }
    }
}
