using Azure.ResourceManager.ServiceBus;

namespace AzuFlow.Azure.Services;

public interface IServiceBusDiscoveryService
{
    IAsyncEnumerable<ServiceBusNamespaceResource> GetNamespacesAsync(CancellationToken ct);
}
