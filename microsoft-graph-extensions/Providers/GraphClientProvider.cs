using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace microsoft_graph_extensions.Providers
{
    public class GraphClientProvider : IGraphClientProvider
    {
        private readonly IDistributedCache _cache;
        public GraphClientProvider(IDistributedCache cache)
        {
            _cache = cache;
        }

        public GraphServiceClient GetGraphClient()
        {
            const string clientId = "SHOULD_BE_CONFIGURE"; //app regis Application (client) ID
            const string clientSecret = "SHOULD_BE_CONFIGURE";
            const string redirectUri = "http://localhost:5001";



            const string tenantId = "SHOULD_BE_CONFIGURE"; //Azure DirectoryId  => AD => Directory properties
            const string authority = "https://login.microsoftonline.com/" + tenantId;

            var appTokenCache = new GraphTokenCacheMemory(tenantId, _cache);

            var cca = new ConfidentialClientApplication(clientId,
                authority, redirectUri, new ClientCredential(clientSecret), null, appTokenCache.GetCacheInstance());

            //// use the default permissions assigned from within the Azure AD app registration portal
            var scopes = new List<string> { "https://graph.microsoft.com/.default" };

            var authenticationProvider = new MsalAuthenticationProvider(tenantId, cca, scopes.ToArray());
            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }
    }
}