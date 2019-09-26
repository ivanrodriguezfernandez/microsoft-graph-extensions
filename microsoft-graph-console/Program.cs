using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using microsoft_graph_extensions.Providers;
using microsoft_graph_extensions.Services;
using Newtonsoft.Json;

namespace microsoft_graph_console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            var collection = new ServiceCollection();
            collection.AddDistributedMemoryCache();

            collection.AddSingleton<IGraphClientProvider, GraphClientProvider>(graphAuth =>
            {
                var provider = collection.BuildServiceProvider();
                var cache = provider.GetService<IDistributedCache>();
                return new GraphClientProvider(cache);
            });

            collection.AddScoped<IGroupService, GroupService>();
            collection.AddScoped<IUserService, UserService>();

            var serviceProvider = collection.BuildServiceProvider();

            var graphClientProvider = serviceProvider.GetService<IGraphClientProvider>();

            var groupService = serviceProvider.GetService<IGroupService>();
            var userService = serviceProvider.GetService<IUserService>();


            const string activeDirectoryGroupId500 = "c7014aa0-f57c-496b-a532-9f7ee2b47839";
            const string activeDirectoryGroupId1000 = "3692ffc0-f083-4a40-bdfe-989b7a65d479";
            const string activeDirectoryGroupId1500 = "7523f52a-368c-4bc1-a5f3-6b191cee9f8e";
            const string activeDirectoryGroupId2000 = "a5af68ff-ddef-4a83-9709-abba941a5857";
            var groupIds = new[]
            {
                activeDirectoryGroupId500, activeDirectoryGroupId1000, activeDirectoryGroupId1500,
                activeDirectoryGroupId2000
            };

            var messages = new List<string>();
            var totalMembers = 1;

            foreach (var groupId in groupIds)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var groupData = await groupService.GetFullData(groupId);
                var msg =
                    $"Get group id {groupId} with DisplayName {groupData.DisplayName} and totalMembers {groupData.Members.Count}";

                foreach (var member in groupData.Members)
                {
                    Console.Write(totalMembers + ":" + groupData.DisplayName);
                    Console.Write(JsonConvert.SerializeObject(member) + "\n");
                    totalMembers++;
                }

                stopWatch.Stop();

                var ts = stopWatch.Elapsed;
                // Format and display the TimeSpan value.
                var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                messages.Add(msg + " :::RunTime " + elapsedTime);

                
            }
            Console.WriteLine("\n*************************************************");
            Console.WriteLine(string.Join("\n", messages));
        }
    }
}