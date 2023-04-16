using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.DAL;
using Grip.DAL.DTO;
using Grip.DAL.Model;
using Grip.Providers;

namespace Grip.Services;

public class AttendanceService
{
    private readonly ILogger<AttendanceService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IStationTokenProvider _stationTokenProvider;

    public AttendanceService(ILogger<AttendanceService> logger, ApplicationDbContext context, IStationTokenProvider stationTokenProvider)
    {
        _logger = logger;
        _context = context;
        _stationTokenProvider = stationTokenProvider;
    }

    public async Task<bool> VerifyPhoneScan(ActiveAttendanceDTO request, DAL.Model.User user)
    {
        // Station number claimed in request 
        int stationNumber = Convert.ToInt32(request.Message.Split("_")[0]);

        _logger.LogInformation($"Phone scan attendance request received for {user.UserName} at station {stationNumber}");
        Station? station = _context.Station.FirstOrDefault(x => x.StationNumber == stationNumber);
        if(station == null)
            throw new Exception("Station not found");
        string? key = station.SecretKey;
        if(key == null)
            throw new Exception("Station does not have a secret key");
        // Calculate local time claimed in request
        DateTime attendanceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        attendanceTime = attendanceTime.AddSeconds(Convert.ToInt32(request.Message.Split("_")[1])).ToLocalTime();
        // Check if the token is valid
        if(_stationTokenProvider.ValidateToken(key,request.Message,request.Token))
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
}