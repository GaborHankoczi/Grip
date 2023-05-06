using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;
using Grip.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Grip.Controllers;


[ApiController]
[Route("api/[controller]")]
public class StationController : ControllerBase
{
    private readonly ILogger<StationController> _logger;
    private readonly IStationService _stationService;
    public StationController(ILogger<StationController> logger, IStationService stationService)
    {
        _logger = logger;
        _stationService = stationService;
    }

    [NotChunked]
    [ValidateApiKey]
    [HttpGet("{StationNumber}/SecretKey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StationSecretKeyDTO>> GetKey([FromRoute] int StationNumber)
    {
        return Ok(await _stationService.GetSecretKey(StationNumber));
    }
}
