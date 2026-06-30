using System.Runtime.CompilerServices;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ServiceBus;
using AzuFlow.Azure.Options;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure.Services;

public class ServiceBusDiscoveryService(ArmClient armClient, IOptions<AzureOptions> options) : IServiceBusDiscoveryService
{
    public async IAsyncEnumerable<ServiceBusNamespaceResource> GetNamespacesAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var subscriptionId = new ResourceIdentifier($"/subscriptions/{options.Value.SubscriptionId}");
        var subscription = armClient.GetSubscriptionResource(subscriptionId);

        await foreach (var ns in subscription.GetServiceBusNamespacesAsync(cancellationToken: ct))
        {
            yield return ns;
        }
    }
}
