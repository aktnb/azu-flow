using AzuFlow.Core.Models;
using AzuFlow.Core.Services;

namespace AzuFlow.Azure.Services;

public class AzureTopologyService(
    IServiceBusDiscoveryService discoveryService,
    TopologyBuilder topologyBuilder) : ITopologyService
{
    public Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken)
    {
        var namespaces = discoveryService.GetNamespacesAsync(cancellationToken);
        return topologyBuilder.BuildFromServiceBusAsync(namespaces, cancellationToken);
    }
}
