using AzuFlow.Core.Models;
using AzuFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzuFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopologyController(ITopologyService topologyService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TopologyGraph>(StatusCodes.Status200OK)]
    public async Task<TopologyGraph> GetAsync(CancellationToken cancellationToken)
    {
        return await topologyService.GetTopologyAsync(cancellationToken);
    }
}
