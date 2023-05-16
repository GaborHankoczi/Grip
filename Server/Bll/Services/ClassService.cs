using AutoMapper;
using Grip.Bll.DTO;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;

namespace Grip.Bll.Services;

/// <summary>
/// Service class for managing classes.
/// </summary>
public class ClassService : IClassService
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="userManager">The user manager instance.</param>
    public ClassService(ApplicationDbContext context, IMapper mapper, UserManager<User> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }
    /// <summary>
    /// Creates a new class.
    /// </summary>
    /// <param name="dto">The DTO for creating a class.</param>
    /// <returns>The created class DTO.</returns>
    public async Task<ClassDTO> Create(CreateClassDTO dto)
    {
        var group = await _context.Groups.FindAsync(dto.GroupId);
        var teacher = await _userManager.FindByIdAsync(dto.TeacherId.ToString());
        if (group == null || teacher == null)
        {
            throw new BadRequestException();
        }
        Class newClass = _mapper.Map<Class>(dto);
        newClass.Group = group;
        newClass.Teacher = teacher;
        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();
        return _mapper.Map<ClassDTO>(newClass);
    }
    /// <summary>
    /// Deletes a class by its ID.
    /// </summary>
    /// <param name="id">The ID of the class to delete.</param>
    public async Task Delete(int id)
    {
        var @class = await _context.Classes.FindAsync(id);
        if (@class == null)
        {
            throw new NotFoundException();
        }

        _context.Classes.Remove(@class);
        await _context.SaveChangesAsync();
    }
    /// <summary>
    /// Retrieves a class by its ID.
    /// </summary>
    /// <param name="id">The ID of the class to retrieve.</param>
    /// <returns>The class DTO.</returns>
    public async Task<ClassDTO> Get(int id)
    {
        var @class = await _context.Classes.Include(c => c.Teacher).Include(c => c.Group).Where(c => c.Id == id).FirstOrDefaultAsync();

        if (@class == null)
        {
            throw new NotFoundException();
        }

        return _mapper.Map<ClassDTO>(@class) with
        {
            Teacher = _mapper.Map<UserInfoDTO>(@class.Teacher),
            Group = _mapper.Map<GroupDTO>(@class.Group)
        };
    }
    /// <summary>
    /// Retrieves all classes.
    /// </summary>
    /// <returns>A list of class DTOs.</returns>
    public async Task<IEnumerable<ClassDTO>> GetAll()
    {
        return (await _context.Classes.Include(c => c.Teacher).Include(c => c.Group).ToListAsync()).Select(
                c => _mapper.Map<ClassDTO>(c)
                with
                {
                    Teacher = _mapper.Map<UserInfoDTO>(c.Teacher),
                    Group = _mapper.Map<GroupDTO>(c.Group)
                }).ToList();
    }
    /// <summary>
    /// Retrieves classes for a user on a specific day.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="date">The date.</param>
    /// <returns>A list of class DTOs.</returns>
    public async Task<IEnumerable<ClassDTO>> GetClassesForUserOnDay(User user, DateOnly date)
    {
        var classes = await _context.Classes
            .Include(c => c.Teacher)
            .Include(c => c.Group)
            .ThenInclude(g => g.Users)
            .Where(c => c.StartDateTime.Year == date.Year && c.StartDateTime.Month == date.Month && c.StartDateTime.Day == date.Day) // filter for searched day
            .Where(c => c.Teacher.Id == user.Id || c.Group.Users.Any(u => u.Id == user.Id)) // filter for classes where user is teacher or student
            .ToListAsync();
        return _mapper.Map<List<ClassDTO>>(classes);
    }

    /// <summary>
    /// Updates a class.
    /// </summary>
    /// <param name="dto">The updated class DTO.</param>
    public async Task Update(ClassDTO dto)
    {
        Class updatedClass = _context.Classes.Find(dto.Id) ?? throw new NotFoundException();

        _mapper.Map(dto, updatedClass);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClassExists(dto.Id))
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
    /// Checks if a class with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the class.</param>
    /// <returns><c>true</c> if the class exists; otherwise, <c>false</c>.</returns>
    private bool ClassExists(int id)
    {
        return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}