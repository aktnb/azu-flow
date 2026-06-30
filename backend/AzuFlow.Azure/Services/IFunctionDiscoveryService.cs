using Azure.ResourceManager.AppService;

namespace AzuFlow.Azure.Services;

public interface IFunctionDiscoveryService
{
    IAsyncEnumerable<WebSiteResource> GetFunctionAppsAsync(CancellationToken ct);
}
