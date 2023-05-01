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
using Grip.DAL.DTO;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

namespace Grip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;       

        public ClassController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClass()
        {
            return (await _context.Classes.Include(c=>c.Teacher).Include(c=>c.Group).ToListAsync()).Select(
                c=>_mapper.Map<ClassDTO>(c) 
                with {
                    Teacher = _mapper.Map<UserInfoDTO>(c.Teacher), 
                    Group = _mapper.Map<GroupDTO>(c.Group)
                    }).ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClassDTO>> GetClass(int id)
        {
            var @class = await _context.Classes.Include(c=>c.Teacher).Include(c=>c.Group).Where(c=>c.Id==id).FirstOrDefaultAsync();

            if (@class == null)
            {
                return NotFound();
            }

            return _mapper.Map<ClassDTO>(@class) with {
                Teacher = _mapper.Map<UserInfoDTO>(@class.Teacher), 
                Group = _mapper.Map<GroupDTO>(@class.Group)
                };
        }

        // PUT: api/Group/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            Class updatedClass = _context.Classes.Find(id);
            if (updatedClass == null)
            {
                return NotFound();
            }
            _mapper.Map(@class, updatedClass);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Group
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClassDTO>> PostClass([FromBody]ClassCreationRequestDTO @class)
        {
            var group = await _context.Groups.FindAsync(@class.GroupId);
            var teacher = await _userManager.FindByIdAsync(@class.TeacherId.ToString());
            if(group==null || teacher==null)
            {
                return BadRequest();
            }
            Class newClass = _mapper.Map<Class>(@class);
            newClass.Group = group;
            newClass.Teacher = teacher;
            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClass", new { id = newClass.Id }, _mapper.Map<ClassDTO>(newClass));
        }

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }

            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClassExists(int id)
        {
            return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
