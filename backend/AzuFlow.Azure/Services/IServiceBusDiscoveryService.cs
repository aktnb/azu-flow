namespace AzuFlow.Azure.Services;

public record ServiceBusNamespaceInfo(
    string FullyQualifiedNamespace,
    string ArmResourceId,
    string ResourceGroup);

public interface IServiceBusDiscoveryService
{
    IAsyncEnumerable<ServiceBusNamespaceInfo> GetNamespacesAsync(CancellationToken ct);
}
