using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;

namespace Grip.Bll.Services
{
    /// <summary>
    /// Service class for managing exempts.
    /// </summary>
    public class ExemptService : IExemptService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptService"/> class.
        /// </summary>
        /// <param name="mapper">The AutoMapper instance.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="userManager">The user manager instance.</param>
        public ExemptService(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
        }
        /// <summary> 
        /// Not used, because user check is requered for autorization, only here for interface implementation
        /// </summary>
        public async Task<ExemptDTO> Create(CreateExemptDTO dto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new exempt.
        /// </summary>
        /// <param name="dto">The DTO for creating an exempt.</param>
        /// <param name="teacher">The teacher who issued the exempt.</param>
        /// <returns>The created exempt DTO.</returns>
        public async Task<ExemptDTO> Create(CreateExemptDTO dto, User teacher)
        {
            var exemptModel = _mapper.Map<Exempt>(dto);
            // TODO validate that issued to is a student
            exemptModel.IssuedBy = teacher;
            _context.Exempt.Add(exemptModel);
            await _context.SaveChangesAsync();
            _context.Users.Where(u => u.Id == dto.IssuedToId).Load();

            return await Get(exemptModel.Id);
        }

        /// <summary>
        /// Deletes an exempt by its ID.
        /// </summary>
        /// <param name="id">The ID of the exempt to delete.</param>
        public async Task Delete(int id)
        {
            var exempt = await _context.Exempt.FindAsync(id);
            if (exempt == null)
            {
                throw new NotFoundException();
            }

            _context.Exempt.Remove(exempt);
            await _context.SaveChangesAsync();

        }

        /// <summary>
        /// Gets an exempt by its ID (doesn't check if the user is authorized to read it).
        /// </summary>
        /// <param name="id">The ID of the exempt to retrieve.</param>
        /// <returns>The exempt DTO.</returns>
        public async Task<ExemptDTO> Get(int id)
        {
            return _mapper.Map<ExemptDTO>(await _context.Exempts.FindAsync(id));
        }

        /// <summary>
        /// Gets an exempt by its ID and checks if the user is authorized to read it.
        /// </summary>
        /// <param name="id">The ID of the exempt to retrieve.</param>
        /// <param name="user">The user making the request.</param>
        /// <returns>The exempt DTO.</returns>
        public async Task<ExemptDTO> GetByUser(int id, User user)
        {
            var exempt = await _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo).Where(e => e.Id == id).FirstOrDefaultAsync();

            if (exempt == null)
            {
                throw new NotFoundException();
            }
            // teachers and admins can read any exempts, students can only read their own
            if (!await _userManager.IsInRoleAsync(user, "Admin") && !await _userManager.IsInRoleAsync(user, "Teacher") && exempt.IssuedTo.Id != user.Id)
                throw new UnauthorizedException();


            return _mapper.Map<ExemptDTO>(exempt);
        }

        /// <summary>
        /// Gets all exempts.
        /// </summary>
        /// <returns>A list of all exempt DTOs.</returns>
        public async Task<IEnumerable<ExemptDTO>> GetAll()
        {
            return (await _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo).ToListAsync()).Select(e => _mapper.Map<ExemptDTO>(e)).ToList();
        }

        /// <summary>
        /// Gets all exempts for a specific user.
        /// </summary>
        /// <param name="user">The user for whom to retrieve the exempts.</param>
        /// <returns>A list of exempt DTOs for the user.</returns>
        public async Task<IEnumerable<ExemptDTO>> GetAllForUser(User user)
        {
            IQueryable<Exempt> query = _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo);
            // if user is not admin or teacher, only return exempts issued to them
            if (!await _userManager.IsInRoleAsync(user, "Admin") && !await _userManager.IsInRoleAsync(user, "Teacher"))
                query = query.Where(e => e.IssuedTo.Id == user.Id);
            return (await query.ToListAsync()).Select(e => _mapper.Map<ExemptDTO>(e)).ToList();
        }

        /// <summary>
        /// Updates an exempt (not implemented).
        /// </summary>
        /// <param name="dto">The updated exempt DTO.</param>
        public Task Update(ExemptDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}