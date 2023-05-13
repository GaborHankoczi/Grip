using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Microsoft.EntityFrameworkCore;
using Server.Bll.Providers;

namespace Server.Bll.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentTimeProvider _currentTimeProvider;
        public StudentService(ApplicationDbContext context, IMapper mapper, ICurrentTimeProvider currentTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _currentTimeProvider = currentTimeProvider;
        }
        /// <summary>
        /// Search students by name and group
        /// </summary>
        /// <param name="name">String that the name of the student should contain, null or empty if this filter shouldn't apply</param>
        /// <param name="groupId">Id of the group the students are part of, null if theus filter shouldn't apply</param>
        /// <returns>IEnumerable containing description of users</returns>
        public async Task<IEnumerable<UserInfoDTO>> SearchStudentsAsync(string? name, int? groupId)
        {
            var userQuery = _context.Users.Include(u => u.Groups).AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                userQuery = userQuery.Where(u => u.UserName.Contains(name));
            }
            if (groupId.HasValue)
            {
                userQuery = userQuery.Where(u => u.Groups.Any(g => g.Id == groupId));
            }
            return (await userQuery.ProjectTo<UserInfoDTO>(_mapper.ConfigurationProvider).ToListAsync());
        }

        /// <summary>
        /// Get details of a student
        /// </summary>
        /// <param name="id">Id of the student</param>
        /// <returns>Detail DTO of the student</returns>
        public async Task<StudentDetailDTO> GetStudentDetailsAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            DateTime currentTime = _currentTimeProvider.Now;
            var absences = await _context.Classes
            .Include(c => c.Group)
            .ThenInclude(g => g.Users)
            .ThenInclude(u => u.Exemptions)
            .Include(c => c.Group)
            .ThenInclude(g => g.Users)
            .ThenInclude(u => u.Attendances)
            .Where(c => c.Group.Users.Any(u => u.Id == id))
            .Where(c => c.StartDateTime <= currentTime)
            .Where(c => !c.Group.Users.First(u => u.Id == id).Attendances.Any(a => a.Time >= c.StartDateTime.AddMinutes(-15) && a.Time <= c.StartDateTime.AddMinutes(15)))
            .Select(c => new AbsenceDTO(
                _mapper.Map<ClassDTO>(c),
                c.Group.Users.First(u => u.Id == id).Exemptions.Any(e => e.ValidFrom <= c.StartDateTime && e.ValidTo >= c.StartDateTime)
            )).ToListAsync();

            return _mapper.Map<StudentDetailDTO>(user) with { Absences = absences };
        }
    }
}