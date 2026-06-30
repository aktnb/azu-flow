namespace AzuFlow.Azure.Options;

public sealed record AzureOptions
{
    private string _subscriptionId = "";

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
}
