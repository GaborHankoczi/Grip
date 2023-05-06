using AutoMapper;
using Castle.Core.Smtp;
using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;

namespace Grip.Bll.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;



        public UserService(ILogger<UserService> logger, ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IMapper mapper, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _emailService = emailService;
        }
        public Task AddRole(int userId, string role)
        {
            throw new NotImplementedException();
        }

        public async Task ConfirmEmail(ConfirmEmailDTO confirmEmailDTO)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDTO.Email);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDTO.Token);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} token verified, email confirmed.");
                var res = await _userManager.AddPasswordAsync(user, confirmEmailDTO.Password);
                if (res.Succeeded)
                {
                    _logger.LogInformation($"User {user.Email} password set successfully.");
                }
                else
                {
                    _logger.LogInformation($"User {user.Email} paswsword not set: {res.Errors.First().Description}");
                    throw new BadRequestException(res.Errors.First().Description);
                }
            }
            else
            {
                throw new BadRequestException("Invalid token.");
            }
        }

        public async Task<UserDTO> Create(RegisterUserDTO user)
        {
            var result = await _userManager.CreateAsync(new User { UserName = user.Name, Email = user.Email });
            if (result.Succeeded)
            {
                var createdUser = await _userManager.FindByEmailAsync(user.Email);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);

                _logger.LogInformation($"New user {user.Name} ({user.Email}) created by admin with activation token: {token}");
                await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Your authentication token is: {token}"); //$"Please confirm your email by clicking this link: {Request.Scheme}://{Request.Host}/api/User/ConfirmEmail?token={token}&email={user.Email}");

                return _mapper.Map<UserDTO>(await _userManager.FindByEmailAsync(user.Email));
            }
            else
            {
                throw new BadRequestException(result.Errors.First().Description);
            }
        }

        public async Task Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} deleted by admin.");
            }
            else
            {
                _logger.LogInformation($"User {user.Email} not deleted by admin: {result.Errors.First().Description}");
                throw new BadRequestException(result.Errors.First().Description);
            }
        }

        public async Task ForgotPassword(ForgotPasswordDTO forgotPassword)
        {
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }
            string result = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendEmailAsync(user.Email, "Reset your password", $"Your authentication token is: {result}");

            _logger.LogInformation($"User {user.Email} forgot password, token generated: {result}");

        }

        public async Task<UserDTO> Get(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAll()
        {
            return _context.Users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
        }

        public async Task<LoginResultDTO> Login(LoginUserDTO login)
        {
            var dbUser = await _userManager.FindByEmailAsync(login.Email);
            if (dbUser == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }
            var result = await _signInManager.PasswordSignInAsync(dbUser, login.Password, true, false);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {login.Email} logged in.");
                var roles = await _userManager.GetRolesAsync(dbUser);
                var loginResult = _mapper.Map<LoginResultDTO>(dbUser) with { Roles = roles.ToArray() };
                return loginResult;
            }
            else if (result.IsLockedOut)
            {
                throw new UnauthorizedException("User is locked out.");
            }
            else if (result.IsNotAllowed)
            {
                throw new UnauthorizedException("User is not allowed to login.");
            }
            throw new UnauthorizedException("Invalid email or password.");
        }

        public async Task RemoveRole(int userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var role = await _roleManager.FindByNameAsync(roleId.ToString());
            if (user == null || role == null)
                throw new NotFoundException("User or role not found.");
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} assigned to role {roleId} by admin.");
            }
            else
            {
                _logger.LogInformation($"User {user.Email} role {roleId} not added by admin: {result.Errors.First().Description}");
                throw new BadRequestException(result.Errors.First().Description);
            }
        }

        public async Task ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }
            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} password reset successfully.");
            }
            else
            {
                throw new BadRequestException("Invalid token.");
            }
        }

        public async Task Update(UserDTO user)
        {
            var dbUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (dbUser == null)
            {
                throw new NotFoundException("User not found.");
            }

            dbUser.UserName = user.UserName;
            dbUser.Email = user.Email;
            // _mapper.Map(user, dbUser); //could be used, but it's not tested
            var result = await _userManager.UpdateAsync(dbUser);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} updated.");
            }
            else
            {
                _logger.LogInformation($"User {user.Email} not updated: {result.Errors.First().Description}");
                throw new BadRequestException(result.Errors.First().Description);
            }
        }
    }
}