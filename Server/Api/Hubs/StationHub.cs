using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.Providers;
using Grip.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Grip.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for station related events
    /// </summary>
    [Authorize("Admin, Teacher, Doorman", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StationHub : Hub<IStationClient>
    {
        /// <summary>
        /// Database context
        /// </summary>
        private readonly ApplicationDbContext _context;

        private readonly ILogger<StationHub> _logger;

        private readonly IMapper _mapper;

        private readonly CurrentTimeProvider _currentTimeProvider;

        /// <summary>
        /// Constructor for the station hub
        /// </summary>
        public StationHub(ApplicationDbContext context, ILogger<StationHub> logger, IMapper mapper, CurrentTimeProvider currentTimeProvider)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _currentTimeProvider = currentTimeProvider;
        }

        /// <summary>
        /// Clients subscribe to station events through this method
        /// </summary>
        /// <param name="stationNumber">Id of the station</param>
        public async Task SelectStation(int stationNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, stationNumber.ToString());
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to station {stationNumber}");

            var currentTime = _currentTimeProvider.Now;

            _context
            .Attendances
            .Where(a => a.Station.Id == stationNumber && a.Time.Year == currentTime.Year && a.Time.Month == currentTime.Month && a.Time.Day == currentTime.Day)
            .Include(a => a.User)
            .Include(a => a.Station)
            .OrderBy(a => a.Time).ToList().ForEach(async a =>
            {
                var dto = new StationScanDTO() { UserInfo = _mapper.Map<UserInfoDTO>(a.User), ScanTime = a.Time, StationId = a.Station.Id };
                await Clients.Caller.ReceiveScan(_mapper.Map<StationScanDTO>(a));
            });

            await Clients.Caller.ReceiveScan(null);
            //TODO send previous scans at station
        }
    }
}