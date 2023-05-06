using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Grip.Bll.Exceptions;

namespace Grip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExemptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IExemptService _exemptService;


        public ExemptController(ApplicationDbContext context, IMapper mapper, UserManager<User> userManager, RoleManager<Role> roleManager, IExemptService exemptService)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _exemptService = exemptService;
        }

        /// <summary>
        /// Retrieves a list of exempt items.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult"/> containing the list of <see cref="ExemptDTO"/> items.
        /// </returns>
        /// <remarks>
        /// This function is accessible via HTTP GET request.
        /// The user must be authorized to access this endpoint.
        /// Returns 200 OK if the request is successful.
        /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
        /// </remarks>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ExemptDTO>>> GetExempt()
        {
            IQueryable<Exempt> query = _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo);
            // if user is not admin or teacher, only return exempts issued to them
            User user = await _userManager.GetUserAsync(User) ?? throw new Exception("User not found");
            if (!await _userManager.IsInRoleAsync(user, "Admin") && !await _userManager.IsInRoleAsync(user, "Teacher"))
                query = query.Where(e => e.IssuedTo.Id == user.Id);
            return (await query.ToListAsync()).Select(e => _mapper.Map<ExemptDTO>(e)).ToList();
        }

        /// <summary>
        /// Get a specific exempt
        /// </summary>
        /// <param name="id">Identifyer of the exempt</param>
        /// <returns>A single exempt</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExemptDTO>> GetExempt(int id)
        {
            var exempt = await _exemptService.Get(id);
            var user = await _userManager.GetUserAsync(User); // get loggegd in user
            // teachers and admins can read any exempts, students can only read their own
            if (!await _userManager.IsInRoleAsync(user, Role.Admin) && !await _userManager.IsInRoleAsync(user, Role.Teacher) && exempt.IssuedTo.Id != user.Id)
                return Unauthorized();

            return exempt;
        }

        /// <summary>
        /// Creates an exempt
        /// </summary>
        /// <param name="exempt">Exempt to create</param>
        /// <returns>Created exempt</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExemptDTO>> PostExempt(CreateExemptDTO exempt)
        {
            var CreatedExempt = await _exemptService.Create(exempt, await _userManager.GetUserAsync(User) ?? throw new NotFoundException("User not found"));

            return CreatedAtAction("GetExempt", new { id = CreatedExempt.Id }, CreatedExempt);
        }

        /// <summary>
        /// Deletes an exempt
        /// </summary>
        /// <param name="id">Id of exempt to delete</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteExempt(int id)
        {
            await _exemptService.Delete(id);

            return NoContent();
        }

        /// <summary>
        /// Checks if a group exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the group to check.</param>
        /// <returns><c>true</c> if the group exists; otherwise, <c>false</c>.</returns>
        private bool ExemptExists(int id)
        {
            return (_context.Exempt?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
