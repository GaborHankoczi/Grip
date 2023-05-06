

using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.Bll.Services.Interfaces;
public interface IAttendanceService
{
    /// <summary>
    /// Register a new attendance by phone scan
    /// </summary>
    /// <param name="request">The request DTO</param>
    /// <param name="user">The user requesting verification</param>
    public Task<bool> VerifyPhoneScan(ActiveAttendanceDTO request, User user);
    /// <summary>
    /// Verify the station scan
    /// </summary>
    /// <param name="request">The request</param>
    public Task<bool> VerifyPassiveScan(PassiveAttendanceDTO request);
}