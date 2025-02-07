using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Controllers;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using System.Linq.Expressions;

namespace TaskManagementSystem.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<TaskManagementContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockUserDbSet;

        public UserControllerTests()
        {
            _mockContext = new Mock<TaskManagementContext>(new DbContextOptions<TaskManagementContext>());
            _mockUserDbSet = new Mock<DbSet<User>>();
            _mockContext.Setup(c => c.Users).Returns(_mockUserDbSet.Object);
        }

        [Fact]
        public async Task Register_ValidInput_ReturnsCreatedResult()
        {
            // Arrange
            var userDto = new RegisterUserDTO 
            { 
                Username = "testuser",
                Email = "test@test.com",
                Password = "password123"
            };

            // Setup mock to return false for any username/email check
            _mockUserDbSet.Setup(db => db.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] args) => null);

            var users = new List<User>().AsQueryable();
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.GetEnumerator()).Returns(users.GetEnumerator());

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.Register(userDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<UserRegDTO>(createdAtActionResult.Value);
            Assert.Equal(userDto.Username, returnValue.Username);
            Assert.Equal(userDto.Email, returnValue.Email);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginDto = new LoginUserDTO
            {
                Username = "testuser",
                Password = "password123"
            };

            var taskItem = new TaskItem
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                Priority = 1,
                Status = Core.Enums.TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                CategoryId = 1
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "password123",
                Email = "test@test.com",
                TaskSubscriptions = new List<TaskSubscription>()
            };

            var subscription = new TaskSubscription
            {
                TaskItemId = 1,
                UserId = 1,
                User = user,
                TaskItem = taskItem,
                SubscribedOn = DateTime.UtcNow
            };

            user.TaskSubscriptions.Add(subscription);

            var users = new List<User> { user }.AsQueryable();
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.GetEnumerator()).Returns(users.GetEnumerator());

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserResponseDTO>(okResult.Value);
            Assert.Equal(user.Username, returnValue.Username);
            Assert.Equal(user.Email, returnValue.Email);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginUserDTO
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "password123",
                Email = "test@test.com",
                TaskSubscriptions = new List<TaskSubscription>()
            };

            var users = new List<User> { user }.AsQueryable();
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.GetEnumerator()).Returns(users.GetEnumerator());

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetUser_ExistingUser_ReturnsUserWithTasks()
        {
            // Arrange
            var userId = 1;
            var taskItem = new TaskItem
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                Priority = 1,
                Status = Core.Enums.TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                CategoryId = 1
            };

            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                Password = "password123",
                TaskSubscriptions = new List<TaskSubscription>()
            };

            var subscription = new TaskSubscription
            {
                TaskItemId = 1,
                UserId = userId,
                User = user,
                TaskItem = taskItem,
                SubscribedOn = DateTime.UtcNow
            };

            user.TaskSubscriptions.Add(subscription);

            var users = new List<User> { user }.AsQueryable();
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.GetEnumerator()).Returns(users.GetEnumerator());

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserResponseDTO>(okResult.Value);
            Assert.Equal(user.Username, returnValue.Username);
            Assert.Single(returnValue.SubscribedTasks);
        }

        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var users = new List<User>().AsQueryable();
            
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(x => x.GetEnumerator()).Returns(users.GetEnumerator());

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}