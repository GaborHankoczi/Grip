using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Grip.Api.Hubs
{
    [Authorize("Admin, Teacher, Doorman")]
    public class StationHub : Hub<IStationClient>
    {
    }
}