using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Grip.Api.Hubs
{
    [Authorize("Admin, Teacher, Doorman")]
    public class StationHub : Hub<IStationClient>
    {
        public async Task SelectStation(int stationNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, stationNumber.ToString());
            //TODO send previos scans at station
        }
    }
}