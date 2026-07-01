using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;
using Azure.ResourceManager.AppService;
using AzuFlow.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzuFlow.Azure.Services;

public class TopologyBuilder(ILogger<TopologyBuilder> logger, TokenCredential credential)
{
    private const int MaxConcurrentResources = 5;

    public async Task<TopologyGraph> BuildAsync(
        IAsyncEnumerable<ServiceBusNamespaceInfo> namespaces,
        IAsyncEnumerable<WebSiteResource> functionApps,
        CancellationToken ct)
    {
        var nsList = new List<ServiceBusNamespaceInfo>();
        await foreach (var ns in namespaces.WithCancellation(ct))
            nsList.Add(ns);

        var appList = new List<WebSiteResource>();
        await foreach (var app in functionApps.WithCancellation(ct))
            appList.Add(app);

        using var semaphore = new SemaphoreSlim(MaxConcurrentResources, MaxConcurrentResources);

        var sbTasks = nsList.Select(ns => ProcessNamespaceAsync(ns, semaphore, ct));

        var sbResults = await Task.WhenAll(sbTasks);

        var sbNodes = sbResults.SelectMany(r => r.Nodes).ToList();
        var sbEdges = sbResults.SelectMany(r => r.Edges).ToList();

        var sbNodeLookup = BuildServiceBusLookup(sbNodes);
        var funcTasks = appList.Select(app => ProcessFunctionAppAsync(app, sbNodeLookup, semaphore, ct));
        var funcResults = await Task.WhenAll(funcTasks);

        return new TopologyGraph
        {
            Nodes = sbNodes.Concat(funcResults.SelectMany(r => r.Nodes)).ToList(),
            Edges = sbEdges.Concat(funcResults.SelectMany(r => r.Edges)).ToList()
        };
    }

    private async Task<(IReadOnlyList<TopologyNode> Nodes, IReadOnlyList<TopologyEdge> Edges)> ProcessNamespaceAsync(
        ServiceBusNamespaceInfo ns,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var adminClient = new ServiceBusAdministrationClient(ns.FullyQualifiedNamespace, credential);
            var queueTask = CollectQueuesAsync(adminClient, ns, ct);
            var topicTask = CollectTopicsAndSubscriptionsAsync(adminClient, ns, ct);
            await Task.WhenAll(queueTask, topicTask);

            return (
                queueTask.Result.Concat(topicTask.Result.Nodes).ToList(),
                topicTask.Result.Edges
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            logger.LogWarning("Failed to process namespace {Namespace}: {Status} {ErrorCode}",
                ns.FullyQualifiedNamespace, ex.Status, ex.ErrorCode);
            return ([], []);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to process Service Bus namespace {Namespace}", ns.FullyQualifiedNamespace);
            return ([], []);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<(IReadOnlyList<TopologyNode> Nodes, IReadOnlyList<TopologyEdge> Edges)> ProcessFunctionAppAsync(
        WebSiteResource app,
        ServiceBusNodeLookup sbLookup,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var rgName = app.Data.Id!.ResourceGroupName ?? "";
            var appName = app.Data.Name;

            var nodes = new List<TopologyNode>();
            var edges = new List<TopologyEdge>();
            var edgeIds = new HashSet<string>();

            await foreach (var func in app.GetSiteFunctions().GetAllAsync(cancellationToken: ct))
            {
                var funcId = func.Data.Id!.ToString();
                // ARM returns function name as "appName/funcName"
                var funcName = func.Data.Name ?? "";
                var slashIdx = funcName.LastIndexOf('/');
                if (slashIdx >= 0)
                    funcName = funcName[(slashIdx + 1)..];

                if (string.IsNullOrEmpty(funcName))
                    continue;

                nodes.Add(new TopologyNode
                {
                    Id = funcId,
                    Type = TopologyNodeType.Function,
                    Name = funcName,
                    ResourceGroup = rgName
                });

                if (func.Data.Config is null)
                {
                    logger.LogDebug("Function {App}/{Func} has no config; isolated worker model or not yet deployed",
                        appName, funcName);
                    continue;
                }

                var bindings = ParseServiceBusBindings(func.Data.Config, appName, funcName);
                foreach (var (sbNodeId, direction) in ResolveBindingEdges(bindings, sbLookup))
                {
                    var (source, target) = direction == "in"
                        ? (sbNodeId, funcId)
                        : (funcId, sbNodeId);

                    var edgeId = $"{source}>{target}";
                    if (edgeIds.Add(edgeId))
                        edges.Add(new TopologyEdge
                        {
                            Id = edgeId,
                            SourceNodeId = source,
                            TargetNodeId = target
                        });
                }
            }

            return (nodes, edges);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            logger.LogWarning("Failed to process function app {App}: {Status} {ErrorCode}",
                app.Data.Name, ex.Status, ex.ErrorCode);
            return ([], []);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to process function app {App}", app.Data.Name);
            return ([], []);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static IEnumerable<(string SbNodeId, string Direction)> ResolveBindingEdges(
        IReadOnlyList<ServiceBusBinding> bindings,
        ServiceBusNodeLookup lookup)
    {
        foreach (var b in bindings)
        {
            if (b.QueueName is not null)
            {
                if (lookup.Queues.TryGetValue(b.QueueName, out var queueNodes))
                    foreach (var n in queueNodes)
                        yield return (n.Id, b.Direction);
            }
            else if (b.TopicName is not null)
            {
                if (b.SubscriptionName is not null)
                {
                    var key = $"{b.TopicName}:{b.SubscriptionName}";
                    if (lookup.Subscriptions.TryGetValue(key, out var subNodes))
                        foreach (var n in subNodes)
                            yield return (n.Id, b.Direction);
                }
                else
                {
                    if (lookup.Topics.TryGetValue(b.TopicName, out var topicNodes))
                        foreach (var n in topicNodes)
                            yield return (n.Id, b.Direction);
                }
            }
        }
    }

    private static ServiceBusNodeLookup BuildServiceBusLookup(IReadOnlyList<TopologyNode> sbNodes)
    {
        var queues = new Dictionary<string, List<TopologyNode>>(StringComparer.OrdinalIgnoreCase);
        var topics = new Dictionary<string, List<TopologyNode>>(StringComparer.OrdinalIgnoreCase);
        var subscriptions = new Dictionary<string, List<TopologyNode>>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in sbNodes)
        {
            switch (node.Type)
            {
                case TopologyNodeType.ServiceBusQueue:
                    queues.GetOrAdd(node.Name).Add(node);
                    break;
                case TopologyNodeType.ServiceBusTopic:
                    topics.GetOrAdd(node.Name).Add(node);
                    break;
                case TopologyNodeType.ServiceBusSubscription:
                    var topicName = ExtractTopicNameFromId(node.Id);
                    if (topicName is not null)
                        subscriptions.GetOrAdd($"{topicName}:{node.Name}").Add(node);
                    break;
            }
        }

        return new ServiceBusNodeLookup(queues, topics, subscriptions);
    }

    private static string? ExtractTopicNameFromId(string resourceId)
    {
        var parts = resourceId.Split('/');
        var idx = Array.IndexOf(parts, "topics");
        return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : null;
    }

    private const int MaxConfigBytes = 1 * 1024 * 1024;

    private IReadOnlyList<ServiceBusBinding> ParseServiceBusBindings(BinaryData config, string appName, string funcName)
    {
        var bindings = new List<ServiceBusBinding>();
        try
        {
            if (config.ToMemory().Length > MaxConfigBytes)
            {
                logger.LogWarning("Function config for {App}/{Func} exceeds size limit, skipping", appName, funcName);
                return bindings;
            }

            using var doc = JsonDocument.Parse(config);
            if (!doc.RootElement.TryGetProperty("bindings", out var bindingsEl))
                return bindings;

            foreach (var b in bindingsEl.EnumerateArray())
            {
                var type = b.TryGetProperty("type", out var t) ? t.GetString() : null;
                if (type is null) continue;

                var isServiceBusBinding =
                    type.Equals("serviceBusTrigger", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("serviceBus", StringComparison.OrdinalIgnoreCase);
                if (!isServiceBusBinding) continue;

                // Determine direction: prefer JSON "direction" property (isolated worker uses "In"/"Out"),
                // fall back to type-based inference (serviceBusTrigger → "in", serviceBus output → "out")
                var isTrigger = type.Equals("serviceBusTrigger", StringComparison.OrdinalIgnoreCase);
                string direction;
                if (b.TryGetProperty("direction", out var dirProp) && dirProp.GetString() is { } dirValue)
                    direction = dirValue.Equals("in", StringComparison.OrdinalIgnoreCase) ? "in" : "out";
                else
                    direction = isTrigger ? "in" : "out";

                var queueName = b.TryGetProperty("queueName", out var q) ? q.GetString() : null;
                var topicName = b.TryGetProperty("topicName", out var tp) ? tp.GetString() : null;
                var subName = b.TryGetProperty("subscriptionName", out var s) ? s.GetString() : null;

                // Isolated worker output binding uses "queueOrTopicName" + "entityType" instead of
                // "queueName" / "topicName". Fall back when neither is set.
                if (queueName is null && topicName is null &&
                    b.TryGetProperty("queueOrTopicName", out var qtProp) &&
                    qtProp.GetString() is { } qtName)
                {
                    var entityType = b.TryGetProperty("entityType", out var etProp)
                        ? etProp.GetString()
                        : null;
                    if (entityType?.Equals("Topic", StringComparison.OrdinalIgnoreCase) == true)
                        topicName = qtName;
                    else
                        queueName = qtName;
                }

                // App Settings references (%VAR_NAME%) cannot be resolved without reading app settings
                if (IsAppSettingRef(queueName) || IsAppSettingRef(topicName))
                {
                    logger.LogDebug(
                        "Function {App}/{Func} uses app setting reference in binding; resolve via app settings for accurate edge mapping",
                        appName, funcName);
                }

                bindings.Add(new ServiceBusBinding(direction, queueName, topicName, subName));
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Malformed function config for {App}/{Func}, skipping binding parse", appName, funcName);
        }
        return bindings;
    }

    private static bool IsAppSettingRef(string? value) =>
        value is { Length: >= 3 } && value[0] == '%' && value[^1] == '%';

    private static async Task<IReadOnlyList<TopologyNode>> CollectQueuesAsync(
        ServiceBusAdministrationClient client,
        ServiceBusNamespaceInfo ns,
        CancellationToken ct)
    {
        var nodes = new List<TopologyNode>();
        await foreach (var queue in client.GetQueuesAsync(ct))
        {
            nodes.Add(new TopologyNode
            {
                Id = $"{ns.EntityIdBase}/queues/{queue.Name}",
                Type = TopologyNodeType.ServiceBusQueue,
                Name = queue.Name,
                ResourceGroup = ns.ResourceGroup
            });
        }
        return nodes;
    }

    private static async Task<(IReadOnlyList<TopologyNode> Nodes, IReadOnlyList<TopologyEdge> Edges)> CollectTopicsAndSubscriptionsAsync(
        ServiceBusAdministrationClient client,
        ServiceBusNamespaceInfo ns,
        CancellationToken ct)
    {
        var nodes = new List<TopologyNode>();
        var edges = new List<TopologyEdge>();

        await foreach (var topic in client.GetTopicsAsync(ct))
        {
            var topicId = $"{ns.EntityIdBase}/topics/{topic.Name}";
            nodes.Add(new TopologyNode
            {
                Id = topicId,
                Type = TopologyNodeType.ServiceBusTopic,
                Name = topic.Name,
                ResourceGroup = ns.ResourceGroup
            });

            await foreach (var sub in client.GetSubscriptionsAsync(topic.Name, ct))
            {
                var subId = $"{topicId}/subscriptions/{sub.SubscriptionName}";
                nodes.Add(new TopologyNode
                {
                    Id = subId,
                    Type = TopologyNodeType.ServiceBusSubscription,
                    Name = sub.SubscriptionName,
                    ResourceGroup = ns.ResourceGroup
                });
                edges.Add(new TopologyEdge
                {
                    Id = $"{topicId}>{subId}",
                    SourceNodeId = topicId,
                    TargetNodeId = subId
                });
            }
        }

        return (nodes, edges);
    }

    private sealed record ServiceBusBinding(
        string Direction,
        string? QueueName,
        string? TopicName,
        string? SubscriptionName);

    private sealed record ServiceBusNodeLookup(
        Dictionary<string, List<TopologyNode>> Queues,
        Dictionary<string, List<TopologyNode>> Topics,
        Dictionary<string, List<TopologyNode>> Subscriptions);
}

file static class DictionaryExtensions
{
    public static List<TValue> GetOrAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key)
        where TKey : notnull
    {
        if (!dict.TryGetValue(key, out var list))
            dict[key] = list = [];
        return list;
    }
}
