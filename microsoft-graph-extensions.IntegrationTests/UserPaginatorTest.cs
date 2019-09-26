using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using microsoft_graph_extensions.Providers;
using microsoft_graph_extensions.Services;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;


namespace microsoft_graph_extensions.IntegrationTests
{
    [TestFixture]
    public class UserPaginatorTest
    {
        private ServiceCollection _collection;
        private IUserService _userService;

        public UserPaginatorTest()
        {
            _collection = new ServiceCollection();
            _collection.AddDistributedMemoryCache();

            _collection.AddSingleton<IGraphClientProvider, GraphClientProvider>(graphAuth =>
            {
                var provider = _collection.BuildServiceProvider();
                var cache = provider.GetService<IDistributedCache>();
                return new GraphClientProvider(cache);
            });

            _collection.AddScoped<IGroupService, GroupService>();
            _collection.AddScoped<IUserService, UserService>();
        }

        [SetUp]
        public void SetUp()
        {
            var serviceProvider = _collection.BuildServiceProvider();
            _userService = serviceProvider.GetService<IUserService>();
        }

        [Test]
        [TestCase(0, 10)]
        [TestCase(1, 11)]
        [TestCase(100, 1000)]
        [TestCase(200, 800)]
        public async Task ShouldReturnUsersPaginatedResult(int skip, int take)
        {
            //var totalUsers = await _userService.GetAllReduceCalls("");
            var result = await _userService.GetPaginatedResult(skip, take, "");
            Assert.AreEqual(take, result.Data.Count());
            Assert.IsTrue(result.Total == 2004);
            Assert.AreEqual(2004,result.Total);
        }
    }
}