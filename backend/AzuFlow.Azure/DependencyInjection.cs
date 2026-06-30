using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using AzuFlow.Azure.Options;
using AzuFlow.Azure.Services;
using AzuFlow.Core.Services;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzuFlow.Azure;

public static class DependencyInjection
{
    public static IServiceCollection AddAzuFlowAzure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureOptions>(configuration.GetSection("Azure"));
        services.Configure<TopologyOptions>(configuration.GetSection("Topology"));
        services.AddSingleton<TokenCredential>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AzureAuthentication");
            var azureOptions = sp.GetRequiredService<IOptions<AzureOptions>>().Value;

            var defaultCredentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = true
            };

            var deviceCodeCredentialOptions = new DeviceCodeCredentialOptions
            {
                DeviceCodeCallback = (deviceCode, cancellationToken) =>
                {
                    logger.LogInformation("Azure device code authentication required: {Message}", deviceCode.Message);
                    Console.WriteLine(deviceCode.Message);
                    return Task.CompletedTask;
                }
            };

            if (azureOptions.TenantId is { } tenantId)
            {
                defaultCredentialOptions.TenantId = tenantId;
                deviceCodeCredentialOptions.TenantId = tenantId;
            }

            return new ChainedTokenCredential(
                new DefaultAzureCredential(defaultCredentialOptions),
                new SuccessfulAuthenticationLoggingCredential(
                    new DeviceCodeCredential(deviceCodeCredentialOptions),
                    logger,
                    "Azure device code authentication succeeded."));
        });
        services.AddSingleton<ArmClient>(sp => new ArmClient(sp.GetRequiredService<TokenCredential>()));
        services.AddSingleton<IServiceBusDiscoveryService, ServiceBusDiscoveryService>();
        services.AddSingleton<IFunctionDiscoveryService, FunctionDiscoveryService>();
        services.AddSingleton<ISupplementalTopologyService, SupplementalTopologyService>();
        services.AddSingleton<TopologyBuilder>();
        services.AddSingleton<ITopologyService, AzureTopologyService>();
        return services;
    }

    private sealed class SuccessfulAuthenticationLoggingCredential(
        TokenCredential inner,
        ILogger logger,
        string message) : TokenCredential
    {
        private int _logged;

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = inner.GetToken(requestContext, cancellationToken);
            LogOnce();
            return token;
        }

        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            var token = await inner.GetTokenAsync(requestContext, cancellationToken);
            LogOnce();
            return token;
        }

        private void LogOnce()
        {
            if (Interlocked.Exchange(ref _logged, 1) == 0)
            {
                logger.LogInformation("{Message}", message);
            }
        }
    }
}
