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
        var subscriptionId = new ResourceIdentifier($"/subscriptions/{options.Value.SubscriptionId}");
        var subscription = armClient.GetSubscriptionResource(subscriptionId);

        await foreach (var ns in subscription.GetServiceBusNamespacesAsync(cancellationToken: ct))
        {
            yield return new ServiceBusNamespaceInfo(
                FullyQualifiedNamespace: $"{ns.Data.Name}.servicebus.windows.net",
                ArmResourceId: ns.Data.Id!.ToString(),
                ResourceGroup: ns.Data.Id!.ResourceGroupName ?? "");
        }
    }
}
