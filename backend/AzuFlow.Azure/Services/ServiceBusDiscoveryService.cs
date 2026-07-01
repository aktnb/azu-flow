using System.Runtime.CompilerServices;
using Azure.ResourceManager;
using AzuFlow.Azure.Options;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure.Services;

public class ServiceBusDiscoveryService(ArmClient armClient, IOptions<AzureOptions> options) : IServiceBusDiscoveryService
{
    public async IAsyncEnumerable<ServiceBusNamespaceInfo> GetNamespacesAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var configured = options.Value.ServiceBusNamespaces;
        if (configured.Count > 0)
        {
            foreach (var fqns in configured)
            {
                var name = fqns.Split('.')[0];
                yield return new ServiceBusNamespaceInfo(
                    FullyQualifiedNamespace: fqns,
                    EntityIdBase: $"servicebus://{fqns}",
                    ResourceGroup: name);
            }
            yield break;
        }

        var subscriptionId = new ResourceIdentifier($"/subscriptions/{options.Value.SubscriptionId}");
        var subscription = armClient.GetSubscriptionResource(subscriptionId);

        await foreach (var ns in subscription.GetServiceBusNamespacesAsync(cancellationToken: ct))
        {
            yield return new ServiceBusNamespaceInfo(
                FullyQualifiedNamespace: $"{ns.Data.Name}.servicebus.windows.net",
                EntityIdBase: ns.Data.Id!.ToString(),
                ResourceGroup: ns.Data.Id!.ResourceGroupName ?? "");
        }
    }
}
