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
    public class ExemptService : IExemptService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

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
        /// Gets exempt by id, DOESN'T check if user is authorized to read it
        /// </summary>
        public async Task<ExemptDTO> Get(int id)
        {
            return _mapper.Map<ExemptDTO>(await _context.Exempts.FindAsync(id));
        }

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

        public async Task<IEnumerable<ExemptDTO>> GetAll()
        {
            return (await _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo).ToListAsync()).Select(e => _mapper.Map<ExemptDTO>(e)).ToList();
        }

        public async Task<IEnumerable<ExemptDTO>> GetAllForUser(User user)
        {
            IQueryable<Exempt> query = _context.Exempt.Include(e => e.IssuedBy).Include(e => e.IssuedTo);
            // if user is not admin or teacher, only return exempts issued to them
            if (!await _userManager.IsInRoleAsync(user, "Admin") && !await _userManager.IsInRoleAsync(user, "Teacher"))
                query = query.Where(e => e.IssuedTo.Id == user.Id);
            return (await query.ToListAsync()).Select(e => _mapper.Map<ExemptDTO>(e)).ToList();
        }

        public Task Update(ExemptDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}