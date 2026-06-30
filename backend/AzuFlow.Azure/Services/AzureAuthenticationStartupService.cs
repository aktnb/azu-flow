using Azure.Core;
using Azure.ResourceManager;
using AzuFlow.Azure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure.Services;

public sealed class AzureAuthenticationStartupService(
    ArmClient armClient,
    IOptions<AzureOptions> options,
    ILogger<AzureAuthenticationStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Azure authentication.");

        try
        {
            var subscriptionId = new ResourceIdentifier($"/subscriptions/{options.Value.SubscriptionId}");
            await armClient.GetSubscriptionResource(subscriptionId).GetAsync(cancellationToken);
            logger.LogInformation("Azure authentication completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure authentication failed during startup.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
