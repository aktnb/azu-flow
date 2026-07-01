namespace AzuFlow.Azure.Options;

public sealed record AzureOptions
{
    private string _subscriptionId = "";
    private string? _tenantId;

    public required string SubscriptionId
    {
        get => _subscriptionId;
        init
        {
            if (!Guid.TryParse(value, out _))
                throw new ArgumentException($"SubscriptionId must be a valid GUID, got: '{value}'.");
            _subscriptionId = value;
        }
    }

    public string? TenantId
    {
        get => _tenantId;
        init
        {
            if (!string.IsNullOrWhiteSpace(value) && !Guid.TryParse(value, out _))
                throw new ArgumentException($"TenantId must be a valid GUID, got: '{value}'.");
            _tenantId = string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public AzureAuthenticationMethod AuthenticationMethod { get; init; } =
        AzureAuthenticationMethod.DefaultThenDeviceCode;

    // When set, namespace discovery skips ARM and connects directly to each FQDN
    // (e.g. "my-ns.servicebus.windows.net"). Requires only Service Bus data-plane permissions.
    public IReadOnlyList<string> ServiceBusNamespaces { get; init; } = [];
}
