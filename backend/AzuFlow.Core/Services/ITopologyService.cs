using AzuFlow.Core.Models;

namespace AzuFlow.Core.Services;

public interface ITopologyService
{
    Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken);
}
