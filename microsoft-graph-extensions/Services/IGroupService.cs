using System.Threading.Tasks;
using Microsoft.Graph;
using microsoft_graph_extensions.Models;

namespace microsoft_graph_extensions.Services
{
    public interface IGroupService
    {
        Task<Group> Get(string groupId);
        Task<GroupModel> GetFullData(string groupId);
        Task AddMemberToGroup(string groupId, string userId);
        Task Delete(string groupId);
    }
}