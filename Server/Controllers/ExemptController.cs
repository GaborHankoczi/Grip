using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using Grip.DAL.DTO.Exempt;
using AutoMapper;
using Grip.DAL.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

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


        public ExemptController(ApplicationDbContext context,IMapper mapper, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/Exempt
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ExemptDTO>>> GetExempt()
        {
            IQueryable<Exempt> query = _context.Exempt.Include(e=>e.IssuedBy).Include(e=>e.IssuedTo);
            // if user is not admin or teacher, only return exempts issued to them
            User user = await _userManager.GetUserAsync(User) ?? throw new Exception("User not found");
            if(!await _userManager.IsInRoleAsync(user,"Admin")&&!await _userManager.IsInRoleAsync(user,"Teacher"))
                query = query.Where(e=>e.IssuedTo.Id==user.Id);
            return (await query.ToListAsync()).Select(e=>_mapper.Map<ExemptDTO>(e)).ToList();
        }

        // GET: api/Exempt/5
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExemptDTO>> GetExempt(int id)
        {
            var exempt = await _context.Exempt.Include(e=>e.IssuedBy).Include(e=>e.IssuedTo).Where(e=>e.Id==id).FirstOrDefaultAsync();
            
            if (exempt == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User); // get loggegd in user
            // teachers and admins can read any exempts, students can only read their own
            if(!await _userManager.IsInRoleAsync(user,"Admin")&&!await _userManager.IsInRoleAsync(user,"Teacher")&&exempt.IssuedTo.Id!=user.Id) 
                return Unauthorized();


            return _mapper.Map<ExemptDTO>(exempt);
        }

        // POST: api/Exempt
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]        
        public async Task<ActionResult<ExemptDTO>> PostExempt(ExemptCreateRequestDTO exempt)
        {
            var exemptModel = _mapper.Map<Exempt>(exempt);
            // TODO validate that issued to is a student
            exemptModel.IssuedBy = await _userManager.GetUserAsync(User) ?? throw new Exception("User not found");
            _context.Exempt.Add(exemptModel);
            await _context.SaveChangesAsync();
            _context.Users.Where(u=>u.Id==exempt.IssuedToId).Load();

            return CreatedAtAction("GetExempt", new { id = exemptModel.Id }, _mapper.Map<ExemptDTO>(exemptModel));
        }

        // DELETE: api/Exempt/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteExempt(int id)
        {
            var exempt = await _context.Exempt.FindAsync(id);
            if (exempt == null)
            {
                return NotFound();
            }

            _context.Exempt.Remove(exempt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExemptExists(int id)
        {
            return (_context.Exempt?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
