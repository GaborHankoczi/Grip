using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;

namespace Grip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassiveTagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPassiveTagService _passiveTagService;

        public PassiveTagController(ApplicationDbContext context, IMapper mapper, IPassiveTagService passiveTagService)
        {
            _context = context;
            _mapper = mapper;
            _passiveTagService = passiveTagService;
        }

        // GET: api/PassiveTag
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PassiveTagDTO>>> GetPassiveTags()
        {
            return Ok(await _passiveTagService.GetAll());
        }

        // GET: api/PassiveTag/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PassiveTagDTO>> GetPassiveTag(int id)
        {
            return await _passiveTagService.Get(id);
        }

        // PUT: api/PassiveTag/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutPassiveTag(int id, [FromBody] UpdatePassiveTagDTO passiveTag)
        {
            if (id != passiveTag.Id)
            {
                return BadRequest();
            }

            await _passiveTagService.Update(passiveTag);

            return NoContent();
        }

        // POST: api/PassiveTag
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PassiveTagDTO>> PostPassiveTag([FromBody] CreatePassiveTagDTO passiveTag)
        {
            var created = await _passiveTagService.Create(passiveTag);

            return CreatedAtAction("GetPassiveTag", new { id = created.Id }, created);
        }

        // DELETE: api/PassiveTag/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePassiveTag(int id)
        {
            await _passiveTagService.Delete(id);

            return NoContent();
        }

        private bool PassiveTagExists(int id)
        {
            return (_context.PassiveTags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
