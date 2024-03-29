using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Grip.Api.Controllers
{
    /// <summary>
    /// Endpoint for managing classes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IClassService _classService;

        /// <summary>
        /// Constructor for the class controller
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userManager">User manager</param>
        /// <param name="mapper">Auto mappre</param>
        /// <param name="classService">Class service</param>
        public ClassController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IClassService classService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _classService = classService;
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
        /// Get all classes
        /// </summary>
        /// <returns>Classes</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClass()
        {
            return Ok(await _classService.GetAll());
        }

        /// <summary>
        /// Get a specific class
        /// </summary>
        /// <param name="id">Identifyer of the class</param>
        /// <returns>A single class</returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClassDTO>> GetClass(int id)
        {
            return Ok(await _classService.Get(id));
        }

        /// <summary>
        /// Update a class
        /// </summary>
        /// <param name="id">Identifyer of the class</param>
        /// <param name="class">The class object</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> PutClass(int id, [FromBody] ClassDTO @class)
        {
            if (id != @class.Id)
            {
                return BadRequest();
            }
            await _classService.Update(@class);

            return NoContent();
        }

        /// <summary>
        /// Creates a new class
        /// </summary>
        /// <param name="class">The class object</param>
        /// <returns>A newly created class</returns>
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClassDTO>> PostClass([FromBody] CreateClassDTO @class)
        {
            var created = await _classService.Create(@class);
            return CreatedAtAction("GetClass", new { id = created.Id }, _mapper.Map<ClassDTO>(created));
        }

        /// <summary>
        /// Deletes a class
        /// </summary>
        /// <param name="id">Identifyer of the class</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteClass(int id)
        {
            await _classService.Delete(id);

            return NoContent();
        }

        /// <summary>
        /// Gets all classes for the logged in user on the given day
        /// </summary>
        /// <param name="date">The date to get classes for</param>
        /// <returns>Classes for the user on the given day</returns>
        [HttpGet("OnDay/{date}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClassOnDay(DateOnly date)
        {
            //var user = await _userManager.GetUserAsync(User) ?? throw new Exception("User logged in, but not found");
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User identifier claim missing");
            var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User logged in, but not found");
            return Ok(await _classService.GetClassesForUserOnDay(user, date));
        }
        private bool ClassExists(int id)
        {
            return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
