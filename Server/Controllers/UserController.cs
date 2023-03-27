using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Grip.DAL;
using Grip.DAL.DTO;
using Grip.DAL.Model;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Grip.Services;

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
    private readonly EmailService _emailService; // TODO add abstraction

    public UserController(ILogger<User> logger, ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IMapper mapper, EmailService emailService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _emailService = emailService;
    }

    [Authorize(Roles="Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<DAL.DTO.UserDTO>> Get()
    {
        return _context.Users.Select(u => _mapper.Map<DAL.DTO.UserDTO>(u)).ToList();
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DAL.DTO.UserDTO>> Get(int id)
    {
        var requester = await _userManager.GetUserAsync(User);
        var user = await _userManager.FindByIdAsync(id.ToString());
        if(requester == null || (requester.Id != id && !await _userManager.IsInRoleAsync(requester, "Admin"))){ // Only admins can get other users
            return Unauthorized();
        }
        if(user == null){
            return NotFound();
        }
        return _mapper.Map<DAL.DTO.UserDTO>(user);
    }

    [Authorize(Roles="Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created,Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async  Task<ActionResult<UserDTO>> Register([FromBody] RegisterUserDTO user)
    {
        var result = await _userManager.CreateAsync(new User{ UserName = user.Name, Email = user.Email });
        if(result.Succeeded){
            var createdUser = await _userManager.FindByEmailAsync(user.Email);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);
            
            _logger.LogInformation($"New user {user.Name} ({user.Email}) created by admin with activation token: {token}");
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Your authentication token is: {token}"); //$"Please confirm your email by clicking this link: {Request.Scheme}://{Request.Host}/api/User/ConfirmEmail?token={token}&email={user.Email}");


            var createdLocation = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{createdUser?.Id}");
            
            return Created(createdLocation,_mapper.Map<UserDTO>(createdUser));
        }
        if(result.Errors.Any()){
            result.Errors.ToList().ForEach(e => ModelState.AddModelError(nameof(RegisterUserDTO.Email), e.Description));
            return BadRequest(ModelState);
        }
        return StatusCode(500);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResultDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ModelStateDictionary))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoginResultDTO>> Login([FromBody] LoginUserDTO user)
    {
        var dbUser = await _userManager.FindByEmailAsync(user.Email);
        if(dbUser == null){
            return NotFound();
        }
        var result = await _signInManager.PasswordSignInAsync(dbUser, user.Password, true, false);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} logged in.");
            var roles = await _userManager.GetRolesAsync(dbUser);                
            var loginResult = _mapper.Map<LoginResultDTO>(dbUser) with {Roles = roles.ToArray()};
            return Ok(loginResult);
        }else if (result.IsLockedOut){                
            ModelState.AddModelError(nameof(LoginUserDTO.Email),"User is locked out.");
        }else if (result.IsNotAllowed){
            ModelState.AddModelError(nameof(LoginUserDTO.Email),"User is not allowed to login.");
        }
        return Unauthorized(ModelState); 
    }
    
    [AllowAnonymous]
    [HttpPost("ConfirmEmail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO confirmEmailDTO)
    {
        var user = await _userManager.FindByEmailAsync(confirmEmailDTO.Email);
        if(user == null){
            return NotFound();
        }
        
        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDTO.Token);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} token verified, email confirmed.");
            var res = await _userManager.AddPasswordAsync(user, confirmEmailDTO.Password);
            if(res.Succeeded){
                _logger.LogInformation($"User {user.Email} password set successfully.");
            }else{
                _logger.LogInformation($"User {user.Email} paswsword not set: {res.Errors.First().Description}");
                ModelState.AddModelError(nameof(ConfirmEmailDTO.Password),res.Errors.First().Description);
            }
            return Ok();
        }else{
            ModelState.AddModelError(nameof(ConfirmEmailDTO.Token),"Invalid token.");
        }
        return BadRequest(ModelState);
    }

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPassword){
        var user = await _userManager.FindByEmailAsync(forgotPassword.Email);
        if(user == null){
            return NotFound();
        }
        string result = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendEmailAsync(user.Email, "Reset your password", $"Your authentication token is: {result}");

        _logger.LogInformation($"User {user.Email} forgot password, token generated: {result}");
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword){
        var user = await _userManager.FindByEmailAsync(resetPassword.Email);
        if(user == null){
            return NotFound();
        }
        var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} password reset successfully.");
            return Ok();
        }else{
            ModelState.AddModelError(nameof(ResetPasswordDTO.Token),"Invalid token.");
        }
        return BadRequest(ModelState);
    }

    [Authorize(Roles="Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if(user == null){
            return NotFound();
        }
        var result = await _userManager.DeleteAsync(user);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} deleted by admin.");
            return Ok();
        }else{
            _logger.LogInformation($"User {user.Email} not deleted by admin: {result.Errors.First().Description}");
            ModelState.AddModelError(nameof(UserDTO.Id),result.Errors.First().Description);
            return BadRequest(ModelState);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> Update(int id, [FromBody] UserDTO user)
    {
        if(user.Id != id){
            ModelState.AddModelError(nameof(UserDTO.Id),"Id in body does not match id in route.");
            return BadRequest(ModelState);
        }
        var dbUser = await _userManager.FindByIdAsync(id.ToString());
        if(dbUser == null){
            return NotFound();
        }
        bool isAdmin = User.IsInRole("Admin");
        var currentUser = await _userManager.GetUserAsync(User);


        if(!isAdmin && dbUser.Id != currentUser?.Id){
            return Unauthorized();            
        }

        if(!isAdmin && dbUser.EmailConfirmed != user.EmailConfirmed){
            ModelState.AddModelError(nameof(UserDTO.EmailConfirmed),"Only admin can change email confirmed.");
            return BadRequest();
        }

        dbUser.UserName = user.UserName;
        dbUser.Email = user.Email;
        var result = await _userManager.UpdateAsync(dbUser);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} updated by " + (isAdmin ? "admin" : "user") + ".");
            return Ok();
        }else{
            _logger.LogInformation($"User {user.Email} not updated: {result.Errors.First().Description}");
            ModelState.AddModelError(nameof(UserDTO.Id),result.Errors.First().Description);
            return BadRequest(ModelState);
        }
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="roleId">Role id</param>    
    [Authorize(Roles="Admin")]
    [HttpPost("AddRole/{userId}/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> AddRole(int userId, string roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());        
        var role = await _roleManager.FindByNameAsync(roleId.ToString());
        if(user==null || role==null)
            return NotFound();
        var result = await _userManager.AddToRoleAsync(user, role.Name);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} assigned to role {roleId} by admin.");
            return Ok();
        }else{
            _logger.LogInformation($"User {user.Email} role {roleId} not added by admin: {result.Errors.First().Description}");
            ModelState.AddModelError(nameof(UserDTO.Id),result.Errors.First().Description);
            return BadRequest(ModelState);
        }
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="roleId">Role id</param>    
    [Authorize(Roles="Admin")]
    [HttpPost("RemoveRole/{userId}/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
    public async Task<ActionResult> RemoveRole(int userId, int roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());        
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if(user==null || role==null)
            return NotFound();
        if(!await _userManager.IsInRoleAsync(user, role.Name))
        {            
            return BadRequest("User is not in role");
        }
        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
        if(result.Succeeded){
            _logger.LogInformation($"User {user.Email} removed from role {roleId} by admin.");
            return Ok();
        }else{
            _logger.LogInformation($"User {user.Email} wasn't removed from role {roleId} by admin: {result.Errors.First().Description}");
            ModelState.AddModelError(nameof(UserDTO.Id),result.Errors.First().Description);
            return BadRequest(ModelState);
        }
    }
}
