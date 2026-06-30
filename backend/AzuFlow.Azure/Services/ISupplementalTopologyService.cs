using AzuFlow.Core.Models;

namespace AzuFlow.Azure.Services;

public interface ISupplementalTopologyService
{
    Task<TopologyGraph?> GetSupplementAsync(CancellationToken cancellationToken);
}
