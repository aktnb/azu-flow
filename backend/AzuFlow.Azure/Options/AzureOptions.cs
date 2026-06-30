namespace AzuFlow.Azure.Options;

public sealed record AzureOptions
{
    public required string SubscriptionId { get; init; }
}
