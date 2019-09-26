using System.Collections.Generic;
using System.Threading.Tasks;
using microsoft_graph_extensions.Models;
using microsoft_graph_extensions.Pagination;

namespace microsoft_graph_extensions.Services
{
    public interface IUserService
    {
        Task Delete(string userId);
        Task Create(string name, string userPrincipalName);
        Task<IEnumerable<UserModel>> GetAll(string filter);
        Task<PaginatedResult<UserModel>> GetPaginatedResult(int skip, int take, string filter);
        Task<IEnumerable<UserModel>> GetAllReduceCalls(string filter);
    }
}