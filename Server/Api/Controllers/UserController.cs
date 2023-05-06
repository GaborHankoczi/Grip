using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Grip.DAL;
using Grip.DAL.Model;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using System.Security.Claims;

namespace Grip.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<User> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;



    public UserController(ILogger<User> logger, ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IMapper mapper, IEmailService emailService, IUserService userService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _emailService = emailService;
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<UserDTO>> Get()
    {
        return _context.Users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDTO>> Get(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user id from token

        var requester = await _userManager.GetUserAsync(User);
        var user = await _userService.Get(id);
        if (requester == null || (requester.Id != id && !await _userManager.IsInRoleAsync(requester, Role.Admin)))
        { // Only admins can get other users
            return Unauthorized();
        }
        return user;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterUserDTO user)
    {
        var created = await _userService.Create(user);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResultDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoginResultDTO>> Login([FromBody] LoginUserDTO user)
    {
        return await _userService.Login(user);
    }

    [AllowAnonymous]
    [HttpPost("ConfirmEmail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO confirmEmailDTO)
    {
        await _userService.ConfirmEmail(confirmEmailDTO);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPassword)
    {
        await _userService.ForgotPassword(forgotPassword);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword)
    {
        await _userService.ResetPassword(resetPassword);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Delete(int id)
    {
        await _userService.Delete(id);
        return Ok();
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> Update(int id, [FromBody] UserDTO user)
    {
        if (id != user.Id)
            return BadRequest();

        await _userService.Update(user);
        return NoContent();
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="roleId">Role id</param>    
    [Authorize(Roles = "Admin")]
    [HttpPost("AddRole/{userId}/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> AddRole(int userId, string roleId)
    {
        await _userService.AddRole(userId, roleId);
        return Ok();
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="roleId">Role id</param>    
    [Authorize(Roles = "Admin")]
    [HttpPost("RemoveRole/{userId}/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult> RemoveRole(int userId, string roleId)
    {
        await _userService.RemoveRole(userId, roleId.ToString());
        return Ok();
    }
}
