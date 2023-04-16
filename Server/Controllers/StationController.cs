using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.DAL;
using Grip.DAL.DTO;
using Grip.DAL.Model;
using Grip.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Grip.Controllers;


[ApiController]
[Route("api/[controller]")]
public class StationController : ControllerBase
{
    private readonly ILogger<StationController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    public StationController(ILogger<StationController> logger, IConfiguration configuration, ApplicationDbContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }

    [NotChunked]
    [ValidateApiKey]
    [HttpGet("{StationNumber}/SecretKey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<StationSecretKeyDTO> GetKey([FromRoute] int StationNumber,[FromHeader] string ApiKey)
    {
        var station = _context.Station.FirstOrDefault(s => s.StationNumber == StationNumber);
        if (station == null)
        {
            if(_configuration["Station:CreateDbEntryOnKeyRequest"]=="True"){
                station = new Station(){
                    StationNumber = StationNumber,
                    SecretKey = Guid.NewGuid().ToString()
                };
                _context.Station.Add(station);
                _context.SaveChanges();
            }else{
                return BadRequest("Station not found");
            }
        }
        return new StationSecretKeyDTO(station.SecretKey);
    }
}
