using AutoMapper;
using Grip.Bll.DTO;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;

namespace Grip.Bll.Services;

public class ClassService : IClassService
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ClassService(ApplicationDbContext context, IMapper mapper, UserManager<User> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }
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
    private bool ClassExists(int id)
    {
        return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}