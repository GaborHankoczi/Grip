using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL.Model;

namespace Grip.Bll.Services
{
    public class GroupService : IGroupService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public GroupService(IMapper mapper, UserManager<User> userManager, ApplicationDbContext context)
        {
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public async Task<GroupDTO> Create(GroupDTO dto)
        {
            Group creating = _mapper.Map<Group>(dto);
            _context.Groups.Add(creating);
            await _context.SaveChangesAsync();

            return await Get(creating.Id);
        }

        public async Task Delete(int id)
        {
            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                throw new NotFoundException();
            }

            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserToGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new NotFoundException();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new NotFoundException();
            }

            group.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new NotFoundException();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new NotFoundException();
            }

            group.Users.Remove(user);
            await _context.SaveChangesAsync();

        }

        public async Task<GroupDTO> Get(int id)
        {
            var @group = await _context.Groups.FindAsync(id);

            if (@group == null)
            {
                throw new NotFoundException();
            }

            return _mapper.Map<GroupDTO>(@group);
        }

        public async Task<IEnumerable<GroupDTO>> GetAll()
        {
            return (await _context.Groups.ToListAsync()).Select(g => _mapper.Map<GroupDTO>(g)).ToList();
        }

        public async Task Update(GroupDTO dto)
        {
            var DbGroup = _context.Groups.Where(g => g.Id == dto.Id).FirstOrDefault();
            if (DbGroup == null)
            {
                throw new NotFoundException();
            }
            _mapper.Map<GroupDTO, Group>(dto, DbGroup);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(dto.Id))
                {
                    throw new NotFoundException();
                }
                else
                {
                    throw new DbConcurrencyException();
                }
            }
        }
        private bool GroupExists(int id)
        {
            return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IEnumerable<UserInfoDTO>> GetUsersInGroup(int groupId)
        {
            return await _context.Groups.Include(g => g.Users).Where(g => g.Id == groupId).SelectMany(g => g.Users).Select(u => _mapper.Map<UserInfoDTO>(u)).ToListAsync();
        }
    }
}