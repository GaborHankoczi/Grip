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

namespace Grip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IClassService _classService;

        public ClassController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IClassService classService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _classService = classService;
        }

        /// <summary>
        /// Get all classes
        /// </summary>
        /// <returns>Classes</returns>
        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClassOnDay(DateOnly date)
        {
            var user = await _userManager.GetUserAsync(User) ?? throw new Exception("User logged in, but not found");
            return Ok(await _classService.GetClassesForUserOnDay(user, date));
        }
        private bool ClassExists(int id)
        {
            return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
