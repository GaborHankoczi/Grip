using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.DAL.DTO;

namespace Grip.Services;

public class AttendanceService
{
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(ILogger<AttendanceService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> VerifyPhoneScan(AttendanceDTO request, DAL.Model.User user)
    {        
        _logger.LogInformation($"Phone scan attendance request received for {user.UserName} at station {request.StationId}");
        
        return true;
    }
    public async Task<bool> VerifyPassiveScan(AttendanceDTO request, DAL.Model.User user)
    {
        _logger.LogInformation($"Passive scan attendance request received for {user.UserName} at station {request.StationId}");

        return true;
    }
}