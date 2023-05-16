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
    /// <summary>
    /// Service class for managing groups.
    /// </summary>
    public class GroupService : IGroupService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the GroupService class.
        /// </summary>
        /// <param name="mapper">The AutoMapper object for object mapping.</param>
        /// <param name="userManager">The UserManager object for user management.</param>
        /// <param name="context">The ApplicationDbContext object for accessing the database context.</param>
        public GroupService(IMapper mapper, UserManager<User> userManager, ApplicationDbContext context)
        {
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }
        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="dto">The GroupDTO object containing the group information.</param>
        /// <returns>The created GroupDTO object.</returns>
        public async Task<GroupDTO> Create(GroupDTO dto)
        {
            Group creating = _mapper.Map<Group>(dto);
            _context.Groups.Add(creating);
            await _context.SaveChangesAsync();

            return await Get(creating.Id);
        }
        /// <summary>
        /// Deletes a group by its ID.
        /// </summary>
        /// <param name="id">The ID of the group to delete.</param>
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
        /// <summary>
        /// Adds a user to a group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="userId">The ID of the user to add.</param>
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
        /// <summary>
        /// Removes a user from a group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="userId">The ID of the user to remove.</param>
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

        /// <summary>
        /// Retrieves a group by its ID.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <returns>The GroupDTO object representing the retrieved group.</returns>
        public async Task<GroupDTO> Get(int id)
        {
            var @group = await _context.Groups.FindAsync(id);

            if (@group == null)
            {
                throw new NotFoundException();
            }

            return _mapper.Map<GroupDTO>(@group);
        }
        /// <summary>
        /// Retrieves all groups.
        /// </summary>
        /// <returns>A collection of GroupDTO objects representing all groups.</returns>
        public async Task<IEnumerable<GroupDTO>> GetAll()
        {
            return (await _context.Groups.ToListAsync()).Select(g => _mapper.Map<GroupDTO>(g)).ToList();
        }

        /// <summary>
        /// Updates a group.
        /// </summary>
        /// <param name="dto">The GroupDTO object containing the updated group information.</param>
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

        /// <summary>
        /// Checks if a group exists based on its ID.
        /// </summary>
        /// <param name="id">The ID of the group to check.</param>
        /// <returns>True if the group exists, otherwise false.</returns>
        private bool GroupExists(int id)
        {
            return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Retrieves all users in a group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>A collection of UserInfoDTO objects representing the users in the group.</returns>
        public async Task<IEnumerable<UserInfoDTO>> GetUsersInGroup(int groupId)
        {
            return await _context.Groups.Include(g => g.Users).Where(g => g.Id == groupId).SelectMany(g => g.Users).Select(u => _mapper.Map<UserInfoDTO>(u)).ToListAsync();
        }
    }
}