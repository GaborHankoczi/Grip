using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;

namespace Grip.Bll.Services
{
    /// <summary>
    /// Provides methods for managing stations.
    /// </summary>
    public class StationService : IStationService
    {
        private readonly ILogger<StationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the StationService class with specified dependencies.
        /// </summary>
        /// <param name="logger">Logger object for logging errors and warnings.</param>
        /// <param name="configuration">Configuration object for accessing app settings.</param>
        /// <param name="context">Database context object for accessing station data.</param>
        public StationService(ILogger<StationService> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Retrieves the secret key for the station with specified station number.
        /// </summary>
        /// <param name="StationNumber">The station number for which to retrieve the secret key.</param>
        /// <returns>A StationSecretKeyDTO object containing the secret key for the specified station.</returns>
        /// <exception cref="BadRequestException">Thrown when station does not exist and configuration flag for creating station on key request is set to false.</exception>
        public async Task<StationSecretKeyDTO> GetSecretKey(int StationNumber)
        {
            var station = _context.Stations.FirstOrDefault(s => s.StationNumber == StationNumber);
            if (station == null)
            {
                if (_configuration["Station:CreateDbEntryOnKeyRequest"] == "True")
                {
                    station = new Station()
                    {
                        StationNumber = StationNumber,
                        SecretKey = Guid.NewGuid().ToString()
                    };
                    _context.Stations.Add(station);
                    _context.SaveChanges();
                }
                else
                {
                    throw new BadRequestException();
                }
            }
            return new StationSecretKeyDTO(station.SecretKey);
        }
    }
}