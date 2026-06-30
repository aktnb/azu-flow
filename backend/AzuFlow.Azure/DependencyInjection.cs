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
            var interactiveBrowserCredentialOptions = new InteractiveBrowserCredentialOptions();

            if (azureOptions.TenantId is { } tenantId)
            {
                defaultCredentialOptions.TenantId = tenantId;
                deviceCodeCredentialOptions.TenantId = tenantId;
                interactiveBrowserCredentialOptions.TenantId = tenantId;
            }

            var defaultCredential = new DefaultAzureCredential(defaultCredentialOptions);
            var deviceCodeCredential = new SuccessfulAuthenticationLoggingCredential(
                new DeviceCodeCredential(deviceCodeCredentialOptions),
                logger,
                "Azure device code authentication succeeded.");
            var interactiveBrowserCredential = new SuccessfulAuthenticationLoggingCredential(
                new InteractiveBrowserCredential(interactiveBrowserCredentialOptions),
                logger,
                "Azure interactive browser authentication succeeded.");

            TokenCredential credential = azureOptions.AuthenticationMethod switch
            {
                AzureAuthenticationMethod.Default => defaultCredential,
                AzureAuthenticationMethod.DeviceCode => deviceCodeCredential,
                AzureAuthenticationMethod.InteractiveBrowser => interactiveBrowserCredential,
                AzureAuthenticationMethod.DefaultThenDeviceCode => new ChainedTokenCredential(
                    defaultCredential,
                    deviceCodeCredential),
                _ => throw new InvalidOperationException(
                    $"Unsupported Azure authentication method: {azureOptions.AuthenticationMethod}.")
            };

            return new InMemoryTokenCachingCredential(credential);
        });
        services.AddSingleton<ArmClient>(sp => new ArmClient(sp.GetRequiredService<TokenCredential>()));
        services.AddSingleton<IServiceBusDiscoveryService, ServiceBusDiscoveryService>();
        services.AddSingleton<IFunctionDiscoveryService, FunctionDiscoveryService>();
        services.AddSingleton<ISupplementalTopologyService, SupplementalTopologyService>();
        services.AddSingleton<TopologyBuilder>();
        services.AddSingleton<ITopologyService, AzureTopologyService>();
        services.AddHostedService<AzureAuthenticationStartupService>();
        return services;
    }

    private sealed class InMemoryTokenCachingCredential(TokenCredential inner) : TokenCredential
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private AccessToken? _cachedToken;
        private string? _cacheKey;

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var key = GetCacheKey(requestContext);

            if (TryGetCachedToken(key, out var token))
            {
                return token;
            }

            _semaphore.Wait(cancellationToken);
            try
            {
                if (TryGetCachedToken(key, out token))
                {
                    return token;
                }

                token = inner.GetToken(requestContext, cancellationToken);
                CacheToken(key, token);
                return token;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            var key = GetCacheKey(requestContext);

            if (TryGetCachedToken(key, out var token))
            {
                return token;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (TryGetCachedToken(key, out token))
                {
                    return token;
                }

                token = await inner.GetTokenAsync(requestContext, cancellationToken);
                CacheToken(key, token);
                return token;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private bool TryGetCachedToken(string key, out AccessToken token)
        {
            if (_cacheKey == key &&
                _cachedToken is { } cachedToken &&
                cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                token = cachedToken;
                return true;
            }

            token = default;
            return false;
        }

        private void CacheToken(string key, AccessToken token)
        {
            _cacheKey = key;
            _cachedToken = token;
        }

        private static string GetCacheKey(TokenRequestContext requestContext) =>
            string.Join(' ', requestContext.Scopes);
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
