using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace microsoft_graph_extensions
{
    public class MsalAuthenticationProvider : IAuthenticationProvider
    {
        
        private ConfidentialClientApplication _clientApplication;
        private string[] _scopes;

        public MsalAuthenticationProvider(string tenantId, ConfidentialClientApplication clientApplication, string[] scopes)
        {
            _clientApplication = clientApplication;
            _scopes = scopes;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        public async Task<string> GetTokenAsync()
        {
            AuthenticationResult authResult = null;
            authResult = await _clientApplication.AcquireTokenForClientAsync(_scopes);
            return authResult.AccessToken;
        }
    }
}