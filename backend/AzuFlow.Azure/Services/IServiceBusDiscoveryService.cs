namespace AzuFlow.Azure.Services;

public record ServiceBusNamespaceInfo(
    string FullyQualifiedNamespace,
    string EntityIdBase,
    string ResourceGroup);

public interface IServiceBusDiscoveryService
{
    IAsyncEnumerable<ServiceBusNamespaceInfo> GetNamespacesAsync(CancellationToken ct);
}
