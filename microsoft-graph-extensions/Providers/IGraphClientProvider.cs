using Microsoft.Graph;

namespace microsoft_graph_extensions.Providers
{
    public interface IGraphClientProvider
    {
        GraphServiceClient GetGraphClient();
    }
}