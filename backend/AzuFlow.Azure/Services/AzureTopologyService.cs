using AzuFlow.Core.Models;
using AzuFlow.Core.Services;

namespace AzuFlow.Azure.Services;

public class AzureTopologyService(
    IServiceBusDiscoveryService serviceBusDiscovery,
    IFunctionDiscoveryService functionDiscovery,
    TopologyBuilder topologyBuilder,
    ISupplementalTopologyService supplementalTopology) : ITopologyService
{
    public async Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken)
    {
        var namespaces = serviceBusDiscovery.GetNamespacesAsync(cancellationToken);
        var functionApps = functionDiscovery.GetFunctionAppsAsync(cancellationToken);
        var discovered = await topologyBuilder.BuildAsync(namespaces, functionApps, cancellationToken);
        var supplement = await supplementalTopology.GetSupplementAsync(cancellationToken);

        return supplement is null ? discovered : Merge(discovered, supplement);
    }

    private static TopologyGraph Merge(TopologyGraph discovered, TopologyGraph supplement)
    {
        var nodes = discovered.Nodes.ToList();
        var nodeIds = nodes.Select(node => node.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var node in supplement.Nodes)
        {
            if (nodeIds.Add(node.Id))
                nodes.Add(node);
        }

        var edges = discovered.Edges.ToList();
        var edgeIds = edges.Select(edge => edge.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var edge in supplement.Edges)
        {
            if (edgeIds.Add(edge.Id))
                edges.Add(edge);
        }

        return new TopologyGraph
        {
            Nodes = nodes,
            Edges = edges,
            GeneratedAt = discovered.GeneratedAt
        };
    }
}
