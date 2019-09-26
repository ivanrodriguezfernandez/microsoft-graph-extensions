using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using microsoft_graph_extensions.Models;
using microsoft_graph_extensions.Pagination;
using microsoft_graph_extensions.Providers;

namespace microsoft_graph_extensions.Services
{
    public class UserService : IUserService
    {
        private readonly IGraphClientProvider _graphClientProvider;

        public UserService(IGraphClientProvider graphClientProvider)
        {
            _graphClientProvider = graphClientProvider;
        }

        public async Task Create(string name, string userPrincipalName)
        {
            var client = _graphClientProvider.GetGraphClient();
            var user = new User
            {
                AccountEnabled = true,
                DisplayName = name,
                MailNickname = name,
                UserPrincipalName = userPrincipalName,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = "password-value#1"
                }
            };
            await client.Users.Request().AddAsync(user);
        }

        public async Task Delete(string userId)
        {
            var client = _graphClientProvider.GetGraphClient();
            await client.Users["{" + userId + "}"]
                .Request()
                .DeleteAsync();
        }

        public async Task<IEnumerable<UserModel>> GetAll(string filter)
        {
            var client = _graphClientProvider.GetGraphClient();
            var options = new List<QueryOption>();

            if (!string.IsNullOrEmpty(filter))
            {
                var filterPrepared = $"startswith(displayName,'{filter}')";
                options.Add(new QueryOption("$filter", filterPrepared));
            }

            var users = await client.Users
                .Request(options)
                .GetAsync();

            var result = new List<UserModel>();
            do
            {
                result.AddRange(users.ToList().Select(x => new UserModel
                {
                    Id = x.Id,
                    UserPrincipalName = x.UserPrincipalName
                }));
            } while (users.NextPageRequest != null && (users = await users.NextPageRequest.GetAsync()).Count > 0);

            return result;
        }

        public async Task<IEnumerable<UserModel>> GetAllReduceCalls(string filter)
        {
            var client = _graphClientProvider.GetGraphClient();

            var options = new List<QueryOption>();

            if (!string.IsNullOrEmpty(filter))
            {
                var filterPrepared = $"startswith(displayName,'{filter}')";
                options.Add(new QueryOption("$filter", filterPrepared));
            }

            var users = await client.Users
                .Request(options)
                .Top(500)
                .GetAsync();

            var result = new List<UserModel>();
            do
            {
                result.AddRange(users.ToList().Select(x => new UserModel
                {
                    Id = x.Id,
                    UserPrincipalName = x.UserPrincipalName
                }));
            } while (users.NextPageRequest != null && (users = await users.NextPageRequest.GetAsync()).Count > 0);

            return result;
        }

        public async Task<PaginatedResult<UserModel>> GetPaginatedResult(int skip, int take, string filter)
        {
            var data = GetPaginatedResultData(skip, take, filter);
            var count = GetPaginatedResultCount(filter);

            var allTasks = new List<Task> {data, count};
            await Task.WhenAll(allTasks);

            return new PaginatedResult<UserModel>
            {
                Data = data.Result,
                Total = count.Result
            };
        }

        private async Task<List<UserModel>> GetPaginatedResultData(int skip, int take, string filter)
        {
            const int graphPaginationLimit = 999;
            const int defaultTop = 500;
            var skipAndTakeResult = skip + take;
            var top = skipAndTakeResult < graphPaginationLimit ? skipAndTakeResult : defaultTop;

            var users = new List<User>();
            var collectionPage = await GetCollectionPage(
                top,
                filter,
                false);

            do
            {
                users.AddRange(collectionPage.ToList());
            } while (skipAndTakeResult >= 1000 && users.Count < skipAndTakeResult &&
                     collectionPage.NextPageRequest != null &&
                     (collectionPage = await collectionPage.NextPageRequest.GetAsync()).Count > 0);

            return users.Skip(skip).Take(take)
                .Select(x => new UserModel {Id = x.Id, UserPrincipalName = x.UserPrincipalName}).ToList();
        }

        private async Task<IGraphServiceUsersCollectionPage> GetCollectionPage(int top, string filter, bool addSelect)
        {
            var client = _graphClientProvider.GetGraphClient();
            var query = client.Users.Request(GetFilter(filter));

            if (addSelect) query = query.Select(x => x.Id);
            return await query.Top(top).GetAsync();
        }

        private async Task<int> GetPaginatedResultCount(string filter)
        {
            var usersCountResult = 0;
            const int maxTop = 999;

            var userCount = await GetCollectionPage(maxTop, filter, true);

            do
            {
                usersCountResult += userCount.Count;
            } while (userCount.NextPageRequest != null &&
                     (userCount = await userCount.NextPageRequest.GetAsync()).Count > 0);

            return usersCountResult;
        }

        private static IEnumerable<QueryOption> GetFilter(string filter)
        {
            var options = new List<QueryOption>();
            if (!string.IsNullOrEmpty(filter))
            {
                var filterPrepared = $"startswith(displayName, '{filter}')" +
                                     $" or startswith(givenName,'{filter}')" +
                                     $" or startswith(surname,'{filter}')" +
                                     $" or startswith(mail,'{filter}')" +
                                     $" or startswith(userPrincipalName,'{filter}')";
                options.Add(new QueryOption("$filter", filterPrepared));
            }

            return options;
        }
    }
}