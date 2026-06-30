using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using AzuFlow.Azure.Options;
using AzuFlow.Azure.Services;
using AzuFlow.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzuFlow.Azure;

public static class DependencyInjection
{
    public static IServiceCollection AddAzuFlowAzure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureOptions>(configuration.GetSection("Azure"));
        services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());
        services.AddSingleton<ArmClient>(sp => new ArmClient(sp.GetRequiredService<TokenCredential>()));
        services.AddSingleton<IServiceBusDiscoveryService, ServiceBusDiscoveryService>();
        services.AddSingleton<TopologyBuilder>();
        services.AddSingleton<ITopologyService, AzureTopologyService>();
        return services;
    }
}
