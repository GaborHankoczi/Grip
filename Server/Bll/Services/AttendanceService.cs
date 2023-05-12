using Grip.DAL;
using Grip.Bll.DTO;
using Grip.DAL.Model;
using Grip.Providers;
using Grip.Bll.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Grip.Bll.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ILogger<AttendanceService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IStationTokenProvider _stationTokenProvider;
    private readonly IMapper _mapper;

    public AttendanceService(ILogger<AttendanceService> logger, ApplicationDbContext context, IStationTokenProvider stationTokenProvider, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _stationTokenProvider = stationTokenProvider;
        _mapper = mapper;
    }

    public async Task<bool> VerifyPhoneScan(ActiveAttendanceDTO request, DAL.Model.User user)
    {
        // Station number claimed in request 
        int stationNumber = Convert.ToInt32(request.Message.Split("_")[0]);

        _logger.LogInformation($"Phone scan attendance request received for {user.UserName} at station {stationNumber}");
        Station? station = _context.Stations.FirstOrDefault(x => x.StationNumber == stationNumber);
        if (station == null)
            throw new Exception("Station not found");
        string? key = station.SecretKey;
        if (key == null)
            throw new Exception("Station does not have a secret key");
        // Calculate local time claimed in request
        DateTime attendanceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        attendanceTime = attendanceTime.AddSeconds(Convert.ToInt32(request.Message.Split("_")[1])).ToLocalTime();
        // Check if the token is valid
        if (_stationTokenProvider.ValidateToken(key, request.Message, request.Token))
        {
            // TODO message should contain the time of the request
            Attendance attendance = new Attendance
            {
                Station = station,
                Time = attendanceTime,
                User = user
            };
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation($"Invalid token for {user.UserName} at station {stationNumber}");
            throw new Exception("Invalid token");
        }


        return true;
    }
    public async Task<bool> VerifyPassiveScan(PassiveAttendanceDTO request)
    {
        //_logger.LogInformation($"Passive scan attendance request received for {user.UserName} at station {request.StationId}");

        return true;
    }

    private class AttendanceQueryResult
    {
        public Class Class { get; set; }
        public bool Present { get; set; }
        public bool HasExempt { get; set; }
        public DateTime? AuthenticationTime { get; set; }
    }

    public async Task<IEnumerable<AttendanceDTO>> GetAttendanceForDay(User user, DateOnly date)
    {
        // Querry all classes for the given day for the user, wether they are present or not, or if thay have a valid exemption, and the time of identification if they were present
        var attendanceDTOs = await _context.Classes
        .Where(c => c.Group.Users.Contains(user) && c.StartDateTime.Date.Year == date.Year && c.StartDateTime.Date.Month == date.Month && c.StartDateTime.Date.Day == date.Day) // On specified day
        .Include(c => c.Group).ThenInclude(g => g.Users) // Classes that the user has
        .Select(
            c =>
            new AttendanceDTO(
                _mapper.Map<ClassDTO>(c),
                c.Group.Users.First(u => u.Id == user.Id).Exemptions.Any(e => e.ValidFrom <= c.StartDateTime && e.ValidTo >= c.StartDateTime),
                c.Group.Users.First(u => u.Id == user.Id).Attendances.Any(a => a.Time >= c.StartDateTime.AddMinutes(-15) && a.Time <= c.StartDateTime.AddMinutes(15)),
                c.Group.Users.First(u => u.Id == user.Id).Attendances.First(a => a.Time >= c.StartDateTime.AddMinutes(-15) && a.Time <= c.StartDateTime.AddMinutes(15)).Time
            )).ToListAsync();
        /*var asd = from c in _context.Classes.Include(c => c.Group).ThenInclude(g => g.Users)
                  where c.Group.Users.Contains(user) && c.StartDateTime.Date.Year == date.Year && c.StartDateTime.Date.Month == date.Month && c.StartDateTime.Date.Day == date.Day
                  select new
                  {
                      Class = c,
                      HasExempt = false,
                      Present = c.Group.Users.First(u => u.Id == user.Id).Attendances.Any(a => a.Time >= c.StartDateTime.AddMinutes(-15) && a.Time <= c.StartDateTime.AddMinutes(15)),
                      AuthenticationTime = (DateTime?)c.Group.Users.First(u => u.Id == user.Id).Attendances.First(a => a.Time >= c.StartDateTime.AddMinutes(-15) && a.Time <= c.StartDateTime.AddMinutes(15)).Time
                  };
        var asdRes = asd.ToList();*/
        return attendanceDTOs;//attendanceDTOs;
    }

}