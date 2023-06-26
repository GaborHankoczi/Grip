using System.Threading.Tasks;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Grip.Controllers;
using Grip.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Tests.Grip.Api.Controllers
{
    [TestFixture]
    public class StationControllerTests
    {
        private Mock<ILogger<StationController>> _loggerMock;
        private Mock<IStationService> _stationServiceMock;
        private StationController _stationController;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<StationController>>();
            _stationServiceMock = new Mock<IStationService>();
            _stationController = new StationController(_loggerMock.Object, _stationServiceMock.Object);
        }

        [Test]
        public async Task GetKey_ValidStationNumber_ReturnsOkResultWithSecretKeyDTO()
        {
            // Arrange
            var stationNumber = 123;
            var expectedDto = new StationSecretKeyDTO("ABC123");

            _stationServiceMock.Setup(s => s.GetSecretKey(stationNumber))
                    .ReturnsAsync(expectedDto);

            // Act
            var result = await _stationController.GetKey(stationNumber, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var resultDto = (StationSecretKeyDTO)okResult.Value;
            Assert.AreEqual(expectedDto.SecretKey, resultDto.SecretKey);
        }
    }
}
