using Azure;
using Azure.ResourceManager.ServiceBus;
using AzuFlow.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzuFlow.Azure.Services;

public class TopologyBuilder(ILogger<TopologyBuilder> logger)
{
    private const int MaxConcurrentNamespaces = 5;

    public async Task<TopologyGraph> BuildFromServiceBusAsync(
        IAsyncEnumerable<ServiceBusNamespaceResource> namespaces,
        CancellationToken ct)
    {
        var nsList = new List<ServiceBusNamespaceResource>();
        await foreach (var ns in namespaces.WithCancellation(ct))
            nsList.Add(ns);

        using var semaphore = new SemaphoreSlim(MaxConcurrentNamespaces, MaxConcurrentNamespaces);
        var tasks = nsList.Select(ns => ProcessNamespaceAsync(ns, semaphore, ct));
        var results = await Task.WhenAll(tasks);

        return new TopologyGraph
        {
            Nodes = results.SelectMany(r => r.Nodes).ToList(),
            Edges = results.SelectMany(r => r.Edges).ToList()
        };
    }

    private async Task<(IReadOnlyList<TopologyNode> Nodes, IReadOnlyList<TopologyEdge> Edges)> ProcessNamespaceAsync(
        ServiceBusNamespaceResource ns,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var rgName = ns.Data.Id!.ResourceGroupName ?? "";
            var queueTask = CollectQueuesAsync(ns, rgName, ct);
            var topicTask = CollectTopicsAndSubscriptionsAsync(ns, rgName, ct);
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
                ns.Data.Name, ex.Status, ex.ErrorCode);
            return ([], []);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to process Service Bus namespace {Namespace}", ns.Data.Name);
            return ([], []);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static async Task<IReadOnlyList<TopologyNode>> CollectQueuesAsync(
        ServiceBusNamespaceResource ns,
        string rgName,
        CancellationToken ct)
    {
        var nodes = new List<TopologyNode>();
        await foreach (var queue in ns.GetServiceBusQueues().GetAllAsync(cancellationToken: ct))
        {
            nodes.Add(new TopologyNode
            {
                Id = queue.Data.Id!.ToString(),
                Type = TopologyNodeType.ServiceBusQueue,
                Name = queue.Data.Name,
                ResourceGroup = rgName
            });
        }
        return nodes;
    }

    private static async Task<(IReadOnlyList<TopologyNode> Nodes, IReadOnlyList<TopologyEdge> Edges)> CollectTopicsAndSubscriptionsAsync(
        ServiceBusNamespaceResource ns,
        string rgName,
        CancellationToken ct)
    {
        var nodes = new List<TopologyNode>();
        var edges = new List<TopologyEdge>();

        await foreach (var topic in ns.GetServiceBusTopics().GetAllAsync(cancellationToken: ct))
        {
            var topicId = topic.Data.Id!.ToString();
            nodes.Add(new TopologyNode
            {
                Id = topicId,
                Type = TopologyNodeType.ServiceBusTopic,
                Name = topic.Data.Name,
                ResourceGroup = rgName
            });

            await foreach (var sub in topic.GetServiceBusSubscriptions().GetAllAsync(cancellationToken: ct))
            {
                var subId = sub.Data.Id!.ToString();
                nodes.Add(new TopologyNode
                {
                    Id = subId,
                    Type = TopologyNodeType.ServiceBusSubscription,
                    Name = sub.Data.Name,
                    ResourceGroup = rgName
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
}
