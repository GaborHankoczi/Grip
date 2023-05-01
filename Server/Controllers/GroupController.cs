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
using Grip.DAL.DTO;

namespace Grip.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GroupController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;


    public GroupController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    // GET: api/Group
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<GroupDTO>>> GetGroup()
    {
        return (await _context.Groups.ToListAsync()).Select(g => _mapper.Map<GroupDTO>(g)).ToList();
    }

    // GET: api/Group/5
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupDTO>> GetGroup(int id)
    {
        var @group = await _context.Groups.FindAsync(id);

        if (@group == null)
        {
            return NotFound();
        }

        return _mapper.Map<GroupDTO>(@group);
    }

    // PUT: api/Group/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]    
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PutGroup(int id,[FromBody] GroupDTO @group)
    {
        if (id != @group.Id)
        {
            return BadRequest();
        }
        var DbGroup = _context.Groups.Where(g => g.Id == id).FirstOrDefault();
        if(DbGroup == null)
        {
            return NotFound();
        }
        _mapper.Map<GroupDTO, Group>(@group, DbGroup);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GroupExists(id))
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]    
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult PostGroup([FromBody] GroupDTO @group)
    {
        Group creating = _mapper.Map<Group>(@group);
        _context.Groups.Add(creating);
        _context.SaveChanges();

        return CreatedAtAction("GetGroup", new { id = creating.Id }, creating);
    }

    // DELETE: api/Group/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var @group = await _context.Groups.FindAsync(id);
        if (@group == null)
        {
            return NotFound();
        }

        _context.Groups.Remove(@group);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{groupId}/AddUser/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddUserToGroup(int groupId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound();
        }

        group.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch("{groupId}/RemoveUser/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveUserFromGroup(int groupId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound();
        }

        group.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool GroupExists(int id)
    {
        return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
