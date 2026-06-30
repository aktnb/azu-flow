using System.Text.Json;
using System.Text.Json.Serialization;
using AzuFlow.Azure.Options;
using AzuFlow.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure.Services;

public sealed class SupplementalTopologyService(
    IOptions<TopologyOptions> options,
    ILogger<SupplementalTopologyService> logger) : ISupplementalTopologyService
{
    private const long MaxSupplementFileBytes = 1 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<TopologyGraph?> GetSupplementAsync(CancellationToken cancellationToken)
    {
        var path = options.Value.SupplementFile;
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var file = new FileInfo(path);
        if (!file.Exists)
        {
            logger.LogInformation("Supplemental topology file {Path} was not found; continuing without one", file.FullName);
            return null;
        }

        if (file.Length > MaxSupplementFileBytes)
            throw new InvalidOperationException(
                $"Supplemental topology file '{file.FullName}' exceeds the {MaxSupplementFileBytes} byte limit.");

        await using var stream = file.OpenRead();
        var graph = await JsonSerializer.DeserializeAsync<TopologyGraph>(stream, JsonOptions, cancellationToken);
        if (graph is null)
            throw new InvalidOperationException($"Supplemental topology file '{file.FullName}' is empty.");

        logger.LogInformation(
            "Loaded supplemental topology from {Path}: {NodeCount} nodes, {EdgeCount} edges",
            file.FullName,
            graph.Nodes.Count,
            graph.Edges.Count);

        return graph;
    }
}
