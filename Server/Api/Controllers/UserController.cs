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

/// <summary>
/// API endpoints for managing user accounts.
/// </summary>
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

    /// <summary>
    /// Gets a list of all users.
    /// </summary>
    /// <returns>The list of users.</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<UserDTO>> Get()
    {
        return _context.Users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
    }

    /// <summary>
    /// Gets a specific user by ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The user.</returns>
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

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="user">The user registration data.</param>
    /// <returns>The created user.</returns>
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

    /// <summary>
    /// Authenticates a user and generates a login token.
    /// </summary>
    /// <param name="user">The user login data.</param>
    /// <returns>The login result containing the authentication token.</returns>
    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResultDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoginResultDTO>> Login([FromBody] LoginUserDTO user)
    {
        return await _userService.Login(user);
    }


    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <param name="confirmEmailDTO">The email confirmation data.</param>
    /// <returns>Ok if the email is confirmed successfully.</returns>
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

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <param name="forgotPassword">The forgot password data.</param>
    /// <returns>Ok if the password reset email is sent successfully.</returns>
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

    /// <summary>
    /// Resets the user's password.
    /// </summary>
    /// <param name="resetPassword">The reset password data.</param>
    /// <returns>Ok if the password is reset successfully.</returns>
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

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>Ok if the user is deleted successfully.</returns>
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

    /// <summary>
    /// Updates a user's information.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="user">The updated user data.</param>
    /// <returns>No content if the user is updated successfully.</returns>
    [Authorize]
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
