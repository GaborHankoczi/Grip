using Grip.DAL;
using Grip.Bll.DTO;
using Grip.DAL.Model;
using Grip.Bll.Services.Interfaces;
using Grip.Bll.Services;
using Grip.Bll.Exceptions;
using Grip.Bll.Providers;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grip.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Grip.Bll.Providers;
using Moq.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Server.Tests;

namespace Tests.Grip.Bll.Services
{
    [TestFixture]
    public class AttendanceServiceTests
    {
        private AttendanceService _attendanceService;
        private Mock<ILogger<AttendanceService>> _loggerMock;
        private Mock<IStationTokenProvider> _stationTokenProviderMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IHubContext<StationHub, IStationClient>> _signalrHubMock;
        private Mock<ICurrentTimeProvider> _currentTimeProviderMock;

        private ApplicationDbContext _context;
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            _context = Utils.CreateTestContext(_connection);

            _context.Database.EnsureCreated();

            _loggerMock = new Mock<ILogger<AttendanceService>>();
            _stationTokenProviderMock = new Mock<IStationTokenProvider>();
            _mapperMock = new Mock<IMapper>();
            _signalrHubMock = new Mock<IHubContext<StationHub, IStationClient>>();
            _currentTimeProviderMock = new Mock<ICurrentTimeProvider>();
            _signalrHubMock.Setup(h => h.Clients.Group(It.IsAny<string>()).ReceiveScan(It.IsAny<StationScanDTO>())).Returns(Task.CompletedTask);
            _attendanceService = new AttendanceService(
                _loggerMock.Object,
                _context,
                _stationTokenProviderMock.Object,
                _mapperMock.Object,
                _signalrHubMock.Object,
                _currentTimeProviderMock.Object
            );
        }


        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [Test]
        public async Task VerifyPhoneScan_ValidRequest_AddsAttendanceAndSendsSignalRMessage()
        {
            // Arrange
            var request = new ActiveAttendanceDTO
            (
             "1_1621434000_100",
             "abc123"
            );
            var user = new User { Id = 1, UserName = "testUser" };
            var station = new Station { Id = 1, StationNumber = 1, SecretKey = "secretKey" };
            var attendanceTime = new DateTime(2021, 5, 19, 12, 0, 0);
            var expectedAttendance = new Attendance
            {
                Station = station,
                Time = attendanceTime,
                User = user
            };
            var expectedStationScanDTO = new StationScanDTO
            {
                StationId = 1,
                ScanTime = attendanceTime,
                UserInfo = new UserInfoDTO(user.Id, user.UserName)
            };

            _context.Stations.Add(station);

            _context.SaveChanges();
            _stationTokenProviderMock.Setup(p => p.ValidateToken(station.SecretKey, request.Message, request.Token))
                .Returns(true);
            _currentTimeProviderMock.SetupGet(p => p.Now).Returns(attendanceTime);
            _mapperMock.Setup(m => m.Map<UserInfoDTO>(user)).Returns(new UserInfoDTO(user.Id, user.UserName));

            // Act
            await _attendanceService.VerifyPhoneScan(request, user);

            // Assert
            Assert.That(_context.Attendances.Count(), Is.EqualTo(1));
            _signalrHubMock.Verify(h => h.Clients.Group(station.StationNumber.ToString()).ReceiveScan(It.IsAny<StationScanDTO>()), Times.Once);
        }

        [Test]
        public void VerifyPhoneScan_InvalidStation_ThrowsException()
        {
            // Arrange
            var request = new ActiveAttendanceDTO
            (
                "1_1621434000",
                "abc123"
            );
            var user = new User { UserName = "testUser" };


            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _attendanceService.VerifyPhoneScan(request, user));
        }

        [Test]
        public void VerifyPhoneScan_StationWithoutSecretKey_ThrowsException()
        {
            // Arrange
            var request = new ActiveAttendanceDTO
            (
                "1_1621434000",
                "abc123"
            );
            var user = new User { UserName = "testUser" };
            var station = new Station { StationNumber = 1, SecretKey = null };

            _context.Stations.Add(station);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _attendanceService.VerifyPhoneScan(request, user));
        }

        [Test]
        public void VerifyPhoneScan_InvalidToken_ThrowsException()
        {
            // Arrange
            var request = new ActiveAttendanceDTO
            (
                "1_1621434000",
                "abc123"
            );
            var user = new User { UserName = "testUser" };
            var station = new Station { StationNumber = 1, SecretKey = "secretKey" };

            _context.Stations.Add(station);
            _stationTokenProviderMock.Setup(p => p.ValidateToken(station.SecretKey, request.Message, request.Token))
                .Returns(false);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _attendanceService.VerifyPhoneScan(request, user));
        }

        [Test]
        public async Task VerifyPassiveScan_ValidRequest_AddsAttendanceAndSendsSignalRMessage()
        {
            // Arrange
            var request = new PassiveAttendanceDTO
            (
                1,
                123
            );
            var user = new User { UserName = "testUser" };
            var station = new Station { Id = 1, SecretKey = "secretKey" };
            var now = DateTime.Now;
            var expectedAttendance = new Attendance
            {
                Station = station,
                Time = now,
                User = user
            };
            var expectedStationScanDTO = new StationScanDTO
            {
                StationId = 1,
                ScanTime = now,
                UserInfo = new UserInfoDTO(user.Id, user.UserName)
            };

            _context.PassiveTags.Add(new PassiveTag { SerialNumber = 123, User = user });

            _context.Stations.Add(station);

            _context.SaveChanges();

            _currentTimeProviderMock.SetupGet(p => p.Now).Returns(now);
            _mapperMock.Setup(m => m.Map<UserInfoDTO>(user)).Returns(new UserInfoDTO(user.Id, user.UserName));

            // Act
            await _attendanceService.VerifyPassiveScan(request);

            // Assert
            Assert.AreEqual(_context.Attendances.Count(), 1);
            _signalrHubMock.Verify(h => h.Clients.Group(request.StationId.ToString()), Times.Once);
        }

        [Test]
        public void VerifyPassiveScan_InvalidStationOrTag_ThrowsNotFoundException()
        {
            // Arrange
            var request = new PassiveAttendanceDTO
            (
                123,
                1
            );

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _attendanceService.VerifyPassiveScan(request));
        }

        [Test]
        public async Task GetAttendanceForDay_ReturnsAttendanceDTOs()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testUser" };
            var teacher = new User { Id = 2, UserName = "teacher" };
            var date = new DateOnly(2021, 5, 19);
            var group = new Group { Id = 1, Name = "Group", Users = new[] { user } };
            var station = new Station { Id = 1, StationNumber = 1, Name = "Station", SecretKey = "secret" };
            user.Groups = new[] { group };
            var class1 = new Class { Id = 1, Name = "Class1", Teacher = teacher, Group = group, StartDateTime = new DateTime(2021, 5, 19, 10, 0, 0), Station = station };
            var class2 = new Class { Id = 2, Name = "Class1", Teacher = teacher, Group = group, StartDateTime = new DateTime(2021, 5, 19, 14, 0, 0), Station = station };
            var attendance1 = new Attendance { Id = 1, Station = station, Time = new DateTime(2021, 5, 19, 9, 55, 0), User = user };
            var attendance2 = new Attendance { Id = 2, Station = station, Time = new DateTime(2021, 5, 19, 14, 10, 0), User = user };

            _context.Classes.Add(class1);
            _context.Classes.Add(class2);

            _context.Users.Add(user);
            _context.Users.Add(teacher);

            _context.Attendances.Add(attendance1);
            _context.Attendances.Add(attendance2);

            user.Attendances = new[] { attendance1, attendance2 };
            _context.Groups.Add(group);

            _context.SaveChanges();
            _mapperMock.Setup(m => m.Map<ClassDTO>(class1))
                .Returns(new ClassDTO(class1.Id, class1.Name, class1.StartDateTime, new UserInfoDTO(3, "Teacher"), new GroupDTO(1, "Group")));
            _mapperMock.Setup(m => m.Map<ClassDTO>(class2))
                .Returns(new ClassDTO(class2.Id, class2.Name, class2.StartDateTime, new UserInfoDTO(3, "Teacher"), new GroupDTO(1, "Group")));

            // Act
            var result = await _attendanceService.GetAttendanceForDay(user, date);

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(class1.StartDateTime, result.First().Class.StartDateTime);
            Assert.AreEqual(attendance1.Time, result.First().AuthenticationTime);
            Assert.AreEqual(class2.StartDateTime, result.Last().Class.StartDateTime);
            Assert.AreEqual(attendance2.Time, result.Last().AuthenticationTime);
        }
    }
}
