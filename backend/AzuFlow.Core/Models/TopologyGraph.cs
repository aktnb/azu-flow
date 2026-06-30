namespace AzuFlow.Core.Models;

public sealed record TopologyGraph
{
    public IReadOnlyList<TopologyNode> Nodes { get; init; } = [];
    public IReadOnlyList<TopologyEdge> Edges { get; init; } = [];
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.Now;
}