namespace AzuFlow.Core.Models;

public sealed record TopologyEdge
{
    public required string Id { get; init; }
    public required string SourceNodeId { get; init; }
    public required string TargetNodeId { get; init; }
}