using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Grip.Bll.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Grip.DAL;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Grip.Middleware;

namespace Grip.Controllers;


/// <summary>
/// Controller for handling attendance
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly ILogger<AttendanceController> _logger;
    private readonly UserManager<DAL.Model.User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IAttendanceService _attendanceService;

    /// <summary>
    /// Constructor for AttendanceController
    /// </summary>
    /// <param name="logger">Logger for logging</param>
    /// <param name="userManager">User manager</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="context">Database context</param>
    /// <param name="attendanceService">Attendance service</param>
    public AttendanceController(ILogger<AttendanceController> logger, UserManager<DAL.Model.User> userManager, IConfiguration configuration, ApplicationDbContext context, IAttendanceService attendanceService)
    {
        _logger = logger;
        _userManager = userManager;
        _configuration = configuration;
        _attendanceService = attendanceService;
        _context = context;
    }

    /// <summary>
    /// Options for the controller
    /// Only used for routing
    /// </summary>
    [HttpOptions]
    public IActionResult Options()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This endpoint is used to authenticate a user when they are physically present at a station with their phone as identification method
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public async Task<IActionResult> ActiveAuthentication([FromBody] ActiveAttendanceDTO request)
    {
        // Currently logged in user
        var User = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found")) ?? throw new Exception("User not found");
        try
        {
            await _attendanceService.VerifyPhoneScan(request, User);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }


    /// <summary>
    /// This endpoint is used to authenticate a user when they are physically present at a station with their passive id tag as identification method
    /// </summary>
    [HttpPost("passive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ValidateApiKey]
    public async Task<IActionResult> PassiveAuthentication([FromBody] PassiveAttendanceDTO request, [FromHeader] string ApiKey /* For swagger documentation only*/)
    {
        await _attendanceService.VerifyPassiveScan(request);


        return Ok();
    }

    /// <summary>
    /// This endpoint is used to get the logged in users classes for given day and wetaher they are present or not
    /// </summary>
    [HttpGet("{date}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AttendanceDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<IEnumerable<AttendanceDTO>>> GetAttendanceForDay(DateOnly date)
    {
        var User = await _userManager.FindByEmailAsync(HttpContext.User.FindFirstValue(ClaimTypes.Email) ?? throw new Exception("User not found")) ?? throw new Exception("User not found");
        var attendance = await _attendanceService.GetAttendanceForDay(User, date);
        return Ok(attendance);
    }

}