using AzuFlow.Core.Models;
using AzuFlow.Core.Services;

namespace AzuFlow.Azure.Services;

public class AzureTopologyService(
    IServiceBusDiscoveryService serviceBusDiscovery,
    IFunctionDiscoveryService functionDiscovery,
    TopologyBuilder topologyBuilder) : ITopologyService
{
    public Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken)
    {
        var namespaces = serviceBusDiscovery.GetNamespacesAsync(cancellationToken);
        var functionApps = functionDiscovery.GetFunctionAppsAsync(cancellationToken);
        return topologyBuilder.BuildAsync(namespaces, functionApps, cancellationToken);
    }
}
