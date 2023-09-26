using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using Server.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Grip.Bll.Services
{
    [TestFixture]
    public class StationServiceTests
    {
        private Mock<ILogger<StationService>> _loggerMock;
        private Mock<IConfiguration> _configurationMock;

        private StationService _stationService;

        private ApplicationDbContext _context;
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            _context = Utils.CreateTestContext(_connection);

            _context.Database.EnsureCreated();
            _loggerMock = new Mock<ILogger<StationService>>();
            _configurationMock = new Mock<IConfiguration>();



            _stationService = new StationService(
                _loggerMock.Object,
                _configurationMock.Object,
                _context
            );
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [Test]
        public async Task GetSecretKey_ExistingStation_ReturnsStationSecretKeyDTO()
        {
            // Arrange
            int stationNumber = 123;
            var existingStation = new Station
            {
                StationNumber = stationNumber,
                SecretKey = "existing-secret-key"
            };

            _context.Stations.Add(existingStation);
            _context.SaveChanges();
            // Act
            var result = await _stationService.GetSecretKey(stationNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(existingStation.SecretKey, result.SecretKey);
        }

        [Test]
        public async Task GetSecretKey_NonExistingStation_CreateDbEntryOnKeyRequestEnabled_ReturnsStationSecretKeyDTO()
        {
            // Arrange
            int stationNumber = 123;


            _configurationMock.Setup(config => config["Station:CreateDbEntryOnKeyRequest"])
                .Returns("True");

            // Act            
            var result = await _stationService.GetSecretKey(stationNumber);

            // Assert            
            Assert.IsNotNull(result);

        }

        [Test]
        public void GetSecretKey_NonExistingStation_CreateDbEntryOnKeyRequestDisabled_ThrowsBadRequestException()
        {
            // Arrange
            int stationNumber = 123;

            _configurationMock.Setup(config => config["Station:CreateDbEntryOnKeyRequest"])
                .Returns("False");

            // Act & Assert
            Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                await _stationService.GetSecretKey(stationNumber);
            });

        }
    }
}
