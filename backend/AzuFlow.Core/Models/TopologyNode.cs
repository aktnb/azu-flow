namespace AzuFlow.Core.Models;

public sealed record TopologyNode
{
    public required string Id { get; init; }
    public required TopologyNodeType Type { get; set; }
    public required string Name { get; init; }
    public required string ResourceGroup { get; init; }
}
