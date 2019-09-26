using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using microsoft_graph_extensions.Providers;
using microsoft_graph_extensions.Services;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private ServiceCollection _collection;
        private IUserService _userService;

        public UserServiceTests()
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
        public async Task ShouldGetAllUsers()
        {
            var users = await _userService.GetAll("");
            var result = users.ToArray();
        }

        [Test]
        public async Task ShouldGetAllUsersReduceCalls()
        {
            var users = await _userService.GetAllReduceCalls("");
            var result = users.ToArray();
        }

        [Test]
        public async Task ShouldCreateUser()
        {
            for (int i = 0; i < 2; i++)
            {
                var userName = $"TestAzure.{i}.{Guid.NewGuid()}";
                var userPrincipalName = userName + "@batmanrodriguez.onmicrosoft.com";
                await _userService.Create(userName, userPrincipalName);
            }
        }

        [Test]
        public async Task ShouldDeleteUser()
        {
            const string name = "TestAzure";
            var users = await _userService.GetAll(name);
            foreach (var user in users)
            {
                await _userService.Delete(user.Id);
            }
        }
    }
}