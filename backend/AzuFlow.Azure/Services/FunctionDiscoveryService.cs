using System.Runtime.CompilerServices;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using AzuFlow.Azure.Options;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure.Services;

public class FunctionDiscoveryService(ArmClient armClient, IOptions<AzureOptions> options) : IFunctionDiscoveryService
{
    public async IAsyncEnumerable<WebSiteResource> GetFunctionAppsAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var subscriptionId = new ResourceIdentifier($"/subscriptions/{options.Value.SubscriptionId}");
        var subscription = armClient.GetSubscriptionResource(subscriptionId);

        await foreach (var site in subscription.GetWebSitesAsync(cancellationToken: ct))
        {
            if (site.Data.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true)
            {
                yield return site;
            }
        }
    }
}
