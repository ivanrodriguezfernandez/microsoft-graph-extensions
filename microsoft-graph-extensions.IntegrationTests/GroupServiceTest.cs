using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using microsoft_graph_extensions.Providers;
using microsoft_graph_extensions.Services;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class GroupServiceTest
    {
        private IGroupService _groupService;
        private IUserService _userService;
        private ServiceCollection _collection;

        private const string ActiveDirectoryGroupId500 = "c7014aa0-f57c-496b-a532-9f7ee2b47839";
        private const string ActiveDirectoryGroupId1000 = "3692ffc0-f083-4a40-bdfe-989b7a65d479";
        private const string ActiveDirectoryGroupId1500 = "7523f52a-368c-4bc1-a5f3-6b191cee9f8e";
        private const string ActiveDirectoryGroupId2000 = "a5af68ff-ddef-4a83-9709-abba941a5857";
        private const string ActiveDirectoryGroupId = "4f2cacce-2e68-47ab-965f-8fcc9605dfd4"; //batman tenant

        public GroupServiceTest()
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
        public void Setup()
        {
            var serviceProvider = _collection.BuildServiceProvider();
            _groupService = serviceProvider.GetService<IGroupService>();
            _userService = serviceProvider.GetService<IUserService>();
        }

        [Test]
        public async Task ShouldReturnGroup()
        {
            var group = await _groupService.Get(ActiveDirectoryGroupId);
            Assert.NotNull(group);
        }
 
        [Test]
        public async Task ShouldReturnFullModelWithAllMembers()
        {
            var group = await _groupService.GetFullData(ActiveDirectoryGroupId2000);
            Assert.IsTrue(group.Members.Count >=2000);
        }

        [Test]
        [Ignore("disable")]
        public async Task ShouldAddMembers()
        {
            const string name = "TestAzure";
            var users = await _userService.GetAll(name);
            var cont = 0;
            foreach (var user in users)
            {
                if (cont > 2001)
                {
                    var groups = new List<string>();
                    if (cont <= 500) groups.Add(ActiveDirectoryGroupId500);
                    if (cont <= 1000) groups.Add(ActiveDirectoryGroupId1000);
                    if (cont <= 1500) groups.Add(ActiveDirectoryGroupId1500);
                    groups.Add(ActiveDirectoryGroupId2000);

                    foreach (var group in groups)
                    {
                        await _groupService.AddMemberToGroup(group, user.Id);
                    }
                }
                cont++;
            }

            var group500 = await _groupService.GetFullData(ActiveDirectoryGroupId500);
            Assert.AreEqual(500,group500.Members.Count);

            var group1000 = await _groupService.GetFullData(ActiveDirectoryGroupId1000);
            Assert.AreEqual(1000, group1000.Members.Count);

            var group1500 = await _groupService.GetFullData(ActiveDirectoryGroupId1500);
            Assert.AreEqual(1500, group1500.Members.Count);

            var group2000 = await _groupService.GetFullData(ActiveDirectoryGroupId2000);
            Assert.AreEqual(2000, group2000.Members.Count);
        }


        [Test]
        public async Task ShouldDeleteGroup()
        {
            const string name = "IdentitySvcAPIGroupTest";
            var groupId = "f43a6610-3eee-438b-bfae-629616c7971e";
             await _groupService.Delete(groupId);
            Assert.True(true);
        }
    }
}