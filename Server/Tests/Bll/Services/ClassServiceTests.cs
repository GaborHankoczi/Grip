using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Server.Tests;

namespace Tests.Grip.Bll.Services.Tests
{
    [TestFixture]
    public class ClassServiceTests
    {
        private Mock<IMapper> _mapperMock;
        private Mock<UserManager<User>> _userManagerMock;
        private IClassService _classService;
        private ApplicationDbContext _context;
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            _context = Utils.CreateTestContext(_connection);

            _context.Database.EnsureCreated();
            _mapperMock = new Mock<IMapper>();
            _userManagerMock = MockUserManager<User>();
            _classService = new ClassService(_context, _mapperMock.Object, _userManagerMock.Object);
        }


        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [Test]
        public async Task Create_WithValidDto_ReturnsCreatedClassDTO()
        {
            // Arrange
            var dto = new CreateClassDTO
            (
                "ClassName",
                new DateTime(2020, 1, 1, 12, 0, 0),
                1,
                1,
                1
            );

            var group = new Group { Id = dto.GroupId, Name = "Group" };
            var teacher = new User { Id = dto.TeacherId, UserName = "Teacher" };
            var station = new Station { Id = dto.StationId, StationNumber = dto.StationId, Name = "Station", SecretKey = "SecretKey" };
            var newClass = new Class { Group = group, Teacher = teacher, Name = dto.Name, StartDateTime = dto.StartDateTime, };

            _context.Groups.Add(group);
            _context.Users.Add(teacher);
            _context.Stations.Add(station);

            _context.SaveChanges();

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.TeacherId.ToString()))
                .ReturnsAsync(teacher);
            _mapperMock.Setup(m => m.Map<Class>(dto))
                .Returns(newClass);
            _mapperMock.Setup(m => m.Map<ClassDTO>(newClass))
                .Returns(new ClassDTO(newClass.Id, newClass.Name, dto.StartDateTime, new UserInfoDTO(teacher.Id, teacher.UserName), new GroupDTO(group.Id, group.Name)));

            // Act
            var result = await _classService.Create(dto);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<ClassDTO>(result);
            Assert.AreEqual(_context.Classes.Count(), 1);

        }

        [Test]
        public void Create_WithInvalidDto_ThrowsBadRequestException()
        {
            // Arrange
            var dto = new CreateClassDTO
            (
                "ClassName",
                new DateTime(2020, 1, 1, 12, 0, 0),
                1,
                1,
                1
            );

            // Act & Assert
            Assert.ThrowsAsync<BadRequestException>(() => _classService.Create(dto));
        }

        [Test]
        public async Task Delete_WithExistingId_DeletesClass()
        {
            // Arrange
            var id = 1;
            var teacher = new User { Id = 1, UserName = "Teacher" };
            var group = new Group { Id = 1, Name = "Group" };
            var station = new Station { Id = 1, StationNumber = 1, SecretKey = "SecretKey", Name = "Station" };
            var existingClass = new Class { Station = station, Id = id, Group = group, Teacher = teacher, Name = "ClassName", StartDateTime = new DateTime(2020, 1, 1, 12, 0, 0) };

            _context.Users.Add(teacher);
            _context.Groups.Add(group);
            _context.Stations.Add(station);
            _context.Classes.Add(existingClass);

            _context.SaveChanges();

            // Act
            await _classService.Delete(id);

            // Assert
            Assert.AreEqual(_context.Classes.Count(), 0);
        }

        [Test]
        public void Delete_WithNonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var id = 1;

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _classService.Delete(id));
        }
        /*
                [Test]
                public async Task Get_WithExistingId_ReturnsClassDTO()
                {
                    // Arrange
                    var id = 1;
                    var existingClass = new Class { Id = id, Teacher = new User() { Id = 1, UserName = "Teacher" }, Group = new Group() { Name = "Group" } };
                    var classDto = new ClassDTO(id, "ClassName", new DateTime(2020, 1, 1, 12, 0, 0), new UserInfoDTO(1, "Teacher"), new GroupDTO(1, "Group"));

                    _contextMock.Setup(c => c.Classes.Include(c => c.Teacher).Include(c => c.Group).Where(c => c.Id == id))
                        .Returns(MockDbSet(new List<Class> { existingClass }));
                    _mapperMock.Setup(m => m.Map<ClassDTO>(existingClass))
                        .Returns(classDto);
                    _mapperMock.Setup(m => m.Map<UserInfoDTO>(existingClass.Teacher))
                        .Returns(new UserInfoDTO(existingClass.Teacher.Id, existingClass.Teacher.UserName));
                    _mapperMock.Setup(m => m.Map<GroupDTO>(existingClass.Group))
                        .Returns(new GroupDTO(existingClass.Group.Id, existingClass.Group.Name));

                    // Act
                    var result = await _classService.Get(id);

                    // Assert
                    Assert.NotNull(result);
                    Assert.IsInstanceOf<ClassDTO>(result);
                    Assert.AreSame(classDto.Id, result.Id);
                }
                */
        /*
                [Test]
                public void Get_WithNonExistingId_ThrowsNotFoundException()
                {
                    // Arrange
                    var id = 1;

                    _contextMock.Setup(c => c.Classes.Include(c => c.Teacher).Include(c => c.Group).Where(c => c.Id == id))
                        .Returns(MockDbSet(new List<Class> { }));

                    // Act & Assert
                    Assert.ThrowsAsync<NotFoundException>(() => _classService.Get(id));
                }
                */
        /*
                [Test]
                public async Task GetClassesForUserOnDay_WithValidData_ReturnsListOfClassDTOs()
                {
                    // Arrange
                    var user = new User();
                    var date = new DateOnly(2023, 5, 19);
                    var existingClasses = new List<Class> { new Class(), new Class() };

                    _contextMock.Setup(c => c.Classes.Include(c => c.Teacher).Include(c => c.Group).ThenInclude(g => g.Users).Where(c => c.StartDateTime.Year == date.Year && c.StartDateTime.Month == date.Month && c.StartDateTime.Day == date.Day))
                        .Returns(MockDbSet(existingClasses));
                    _mapperMock.Setup(m => m.Map<List<ClassDTO>>(existingClasses))
                        .Returns(existingClasses.Select(c => new ClassDTO(c.Id, c.Name, c.StartDateTime, new UserInfoDTO(c.Teacher.Id, c.Teacher?.UserName ?? "Name"), new GroupDTO(c.Group.Id, c.Group.Name))).ToList());

                    // Act
                    var result = await _classService.GetClassesForUserOnDay(user, date);

                    // Assert
                    Assert.NotNull(result);
                    Assert.IsInstanceOf<IEnumerable<ClassDTO>>(result);
                    Assert.AreEqual(existingClasses.Count, result.Count());
                }
        */
        [Test]
        public async Task Update_WithExistingDto_UpdatesClass()
        {
            // Arrange
            var dto = new ClassDTO(1, "ClassName", new DateTime(2020, 1, 1, 12, 0, 0), new UserInfoDTO(1, "Teacher"), new GroupDTO(1, "Group"));
            var teacher = new User { Id = 1, UserName = "Teacher" };
            var group = new Group { Id = 1, Name = "Group" };
            var station = new Station { Id = 1, StationNumber = 1, Name = "Station", SecretKey = "SecretKey" };
            var existingClass = new Class() { Group = group, Teacher = teacher, Station = station, Id = 1, Name = "OldName" };

            _context.Users.Add(teacher);
            _context.Groups.Add(group);
            _context.Stations.Add(station);
            _context.Classes.Add(existingClass);

            _context.SaveChanges();

            _mapperMock.Setup(m => m.Map(dto, existingClass));

            // Act
            await _classService.Update(dto);

            // Assert
            _mapperMock.Verify(h => h.Map(dto, existingClass), Times.Once);
        }

        [Test]
        public void Update_WithNonExistingDto_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new ClassDTO(1, "ClassName", new DateTime(2020, 1, 1, 12, 0, 0), new UserInfoDTO(1, "Teacher"), new GroupDTO(1, "Group"));

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _classService.Update(dto));
        }

        private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static DbSet<TEntity> MockDbSet<TEntity>(IEnumerable<TEntity> data) where TEntity : class
        {
            var queryableData = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<TEntity>>();
            mockDbSet.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockDbSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockDbSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockDbSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());
            return mockDbSet.Object;
        }
    }
}
