

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
    public Task VerifyPhoneScan(ActiveAttendanceDTO request, User user);
    /// <summary>
    /// Verify the station scan
    /// </summary>
    /// <param name="request">The request</param>
    public Task VerifyPassiveScan(PassiveAttendanceDTO request);

    /// <summary>
    /// Get all attendances for a user on a specific day
    /// </summary>
    /// <param name="user">User used for querrying</param>
    /// <param name="date">Date for querrying</param>
    /// <returns>List of AttendanceDTO-s</returns>
    public Task<IEnumerable<AttendanceDTO>> GetAttendanceForDay(User user, DateOnly date);
}