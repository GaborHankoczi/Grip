using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Grip.DAL.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Grip.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly ILogger<AttendanceController> _logger;
    private readonly UserManager<DAL.Model.User> _userManager;
    private readonly IConfiguration _configuration;

    public AttendanceController(ILogger<AttendanceController> logger, UserManager<DAL.Model.User> userManager, IConfiguration configuration)
    {
        _logger = logger;
        _userManager = userManager;
        _configuration = configuration;
    }

    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public async Task<IActionResult> ActiveAuthentication([FromBody] AttendanceDTO request)
    {
        var User = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? throw new Exception("User not found")) ?? throw new Exception("User not found");
        

        return Ok();
    }

    [HttpPost("passive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public async Task<IActionResult> PassiveAuthentication([FromBody] AttendanceDTO request, [FromHeader] string apiKey)
    {
        if(apiKey != _configuration["ApiKey"])
            return BadRequest("Invalid API Key");
        var User = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? throw new Exception("User not found")) ?? throw new Exception("User not found");
        

        return Ok();
    }
    
}