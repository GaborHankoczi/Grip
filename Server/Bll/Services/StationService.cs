using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;

namespace Grip.Bll.Services
{
    public class StationService : IStationService
    {
        private readonly ILogger<StationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public StationService(ILogger<StationService> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<StationSecretKeyDTO> GetSecretKey(int StationNumber)
        {
            var station = _context.Stations.FirstOrDefault(s => s.StationNumber == StationNumber);
            if (station == null)
            {
                if(_configuration["Station:CreateDbEntryOnKeyRequest"]=="True"){
                    station = new Station(){
                        StationNumber = StationNumber,
                        SecretKey = Guid.NewGuid().ToString()
                    };
                    _context.Stations.Add(station);
                    _context.SaveChanges();
                }else{
                    throw new BadRequestException();
                }
            }
            return new StationSecretKeyDTO(station.SecretKey);
        }
    }
}