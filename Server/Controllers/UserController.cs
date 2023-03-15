using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Grip.DAL;
using Grip.DAL.DTO;
using Grip.Model;
using AutoMapper;

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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserController(ILogger<User> logger, ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    [Authorize(Roles="Admin")]
    [HttpGet]
    public ActionResult<IEnumerable<DAL.DTO.UserDTO>> Get()
    {
        return _context.Users.Select(u => _mapper.Map<DAL.DTO.UserDTO>(u)).ToList();
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<DAL.DTO.UserDTO>> Get(string id)
    {
        var requester = await _userManager.GetUserAsync(User);
        var user = await _userManager.FindByIdAsync(id);
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async  Task<ActionResult<UserDTO>> Register([FromBody] RegisterUserDTO user)
    {
        if (ModelState.IsValid)
        {
            var result = _userManager.CreateAsync(new Model.User{ UserName = user.Name, Email = user.Email });
            if(result.Result.Succeeded){
                var createdUser = await _userManager.FindByEmailAsync(user.Email);
                _logger.LogInformation($"New user {user.Name} ({user.Email}) created by admin.");
                //Todo send email
                var createdLocation = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{createdUser?.Id}");
                
                return Created(createdLocation,_mapper.Map<UserDTO>(createdUser));
            }
            return BadRequest(result.Exception); // TODO maybe shouldn't be used in production
        }
        return BadRequest(ModelState);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginUserDTO user)
    {
        if (ModelState.IsValid)
        {
            var dbUser = await _userManager.FindByEmailAsync(user.Email);
            if(dbUser == null){
                return Unauthorized("User not found.");
            }
            var result = await _signInManager.PasswordSignInAsync(dbUser, user.Password, true, false);
            if(result.Succeeded){
                _logger.LogInformation($"User {user.Email} logged in.");
                return Ok();
            }else if (result.IsLockedOut){                
                return Unauthorized("User is locked out.");
            }else if (result.IsNotAllowed){
                return Unauthorized("User is not allowed to login.");
            }
            return Unauthorized(); 
        }
        return BadRequest(ModelState);
    }
    /*[AllowAnonymous]
    [HttpPatch]
    public async Task<ActionResult> CreateAdminUser()
    {
        var result = await _userManager.CreateAsync(new User{UserName = "Admin", Email = "admin@localhost"}, "Admin123!");
        if(result.Succeeded){
            var role = await _roleManager.FindByNameAsync("Admin");
            if(role == null){
                role = new IdentityRole("Admin");
                await _roleManager.CreateAsync(role);
            }
            await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync("Admin"), "Admin");
            return Ok();
        }
        return BadRequest(result.Errors);
    }*/
}
