using System.Threading.Tasks;
using Microsoft.Graph;
using microsoft_graph_extensions.Models;
using microsoft_graph_extensions.Providers;

namespace microsoft_graph_extensions.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGraphClientProvider _graphClientProvider;

        public GroupService(IGraphClientProvider graphClientProvider)
        {
            _graphClientProvider = graphClientProvider;
        }

        public async Task<Group> Get(string groupId)
        {
            var client = _graphClientProvider.GetGraphClient();
            var result = await client
                .Groups[groupId]
                .Request().Expand(x => x.Members)
                .GetAsync();
            return result;
        }

        public async Task Delete(string groupId)
        {
            var client = _graphClientProvider.GetGraphClient();
            await client.Groups["{" + groupId + "}"]
                .Request()
                .DeleteAsync();
        }

        public async Task<GroupModel> GetFullData(string groupId)
        {
            var graphClient = _graphClientProvider.GetGraphClient();

            var group = await graphClient
                .Groups[groupId]
                .Request().Expand(x => x.Members)
                .GetAsync();

            var data = new GroupModel
            {
                Id = group.Id,
                DisplayName = group.DisplayName
            };

            var users = await graphClient.Groups[group.Id].Members.Request().GetAsync();
            do
            {
                foreach (var user in users)
                {
                    if (user.GetType() == typeof(User))
                    {
                        data.Members.Add(new Member { Display = ((User)user).DisplayName, Value = ((User)user).Id });
                    }
                }
                    
            } while (users.NextPageRequest != null && (users = await users.NextPageRequest.GetAsync()).Count > 0);

            return data;
        }

        public async Task AddMemberToGroup(string groupId, string userId)
        {
            var client = _graphClientProvider.GetGraphClient();
            User userToAdd = await client.Users[userId].Request().GetAsync();
            await client.Groups[groupId].Members.References.Request().AddAsync(userToAdd);
        }
    }
}