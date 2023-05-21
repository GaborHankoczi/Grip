using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Grip.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for station related events
    /// </summary>
    [Authorize("Admin, Teacher, Doorman")]
    public class StationHub : Hub<IStationClient>
    {
        /// <summary>
        /// Clients subscribe to station events through this method
        /// </summary>
        /// <param name="stationNumber">Id of the station</param>
        public async Task SelectStation(int stationNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, stationNumber.ToString());
            //TODO send previous scans at station
        }
    }
}