using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using Grip.DAL.DTO.PassiveTag;
using AutoMapper;
using Grip.DAL.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Grip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassiveTagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;        

        public PassiveTagController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/PassiveTag
        [HttpGet]        
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PassiveTagDTO>>> GetPassiveTags()
        {
            return (await _context.PassiveTags.Include(p=>p.User).ToListAsync())
            .Select(
                p=>_mapper.Map<PassiveTagDTO>(p) 
                with { User = _mapper.Map<UserInfoDTO>(p.User) }).ToList();
        }

        // GET: api/PassiveTag/5
        [HttpGet("{id}")]        
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PassiveTagDTO>> GetPassiveTag(int id)
        {
            var passiveTag = await _context.PassiveTags.Include(p=>p.User).Where(p=>p.Id==id).FirstOrDefaultAsync();

            if (passiveTag == null)
            {
                return NotFound();
            }
            return _mapper.Map<PassiveTagDTO>(passiveTag) with { User = _mapper.Map<UserInfoDTO>(passiveTag.User) };
        }

        // PUT: api/PassiveTag/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutPassiveTag(int id,[FromBody] PassiveTagUpdateRequestDTO passiveTag)
        {
            if (id != passiveTag.Id)
            {
                return BadRequest();
            }

            var passiveTagEntity = await _context.PassiveTags.Include(p=>p.User).Where(p=>p.Id==id).FirstOrDefaultAsync();
            if(_context.PassiveTags.Any(p=>p.SerialNumber == passiveTag.SerialNumber && p.Id != id))
            {
                return BadRequest("PassiveTag with this Serial Number already exists");
            }
            if (passiveTagEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(passiveTag,passiveTagEntity);
            if(passiveTagEntity.User.Id != passiveTag.UserId)
            {
                var user = await _context.Users.FindAsync(passiveTag.UserId);
                if(user == null)
                {
                    return NotFound();
                }
                passiveTagEntity.User = user;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PassiveTagExists(id))
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

        // POST: api/PassiveTag
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]        
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PassiveTagDTO>> PostPassiveTag([FromBody] PassiveTagCreationRequestDTO passiveTag)
        {
            var passiveTagToCreate = _mapper.Map<PassiveTag>(passiveTag);
            if(_context.PassiveTags.Any(p=>p.SerialNumber == passiveTagToCreate.SerialNumber))
            {
                return BadRequest("PassiveTag with this Serial Number already exists");
            }
            var user = await _context.Users.FindAsync(passiveTag.UserId);
            if(user == null)
            {
                return NotFound();
            }
            passiveTagToCreate.User = user;
            _context.PassiveTags.Add(passiveTagToCreate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPassiveTag", new { id = passiveTagToCreate.Id }, _mapper.Map<PassiveTagDTO>(passiveTagToCreate) with { User = _mapper.Map<UserInfoDTO>(passiveTagToCreate.User) });
        }

        // DELETE: api/PassiveTag/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePassiveTag(int id)
        {
            var passiveTag = await _context.PassiveTags.FindAsync(id);
            if (passiveTag == null)
            {
                return NotFound();
            }

            _context.PassiveTags.Remove(passiveTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PassiveTagExists(int id)
        {
            return (_context.PassiveTags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
