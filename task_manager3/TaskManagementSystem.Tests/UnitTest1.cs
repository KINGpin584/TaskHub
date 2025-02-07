using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API;
using TaskManagementSystem.API.Controllers;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Core;
using Microsoft.AspNetCore.SignalR;
using TaskManagementSystem.API.Hubs;
using TaskManagementSystem.API.Services;
using System.Collections.Generic;

namespace TaskManagementSystem.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<TaskManagementContext> _mockContext;
        private readonly Mock<TaskPriorityService> _mockPriorityService;
        private readonly Mock<IHubContext<TaskHub>> _mockHubContext;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<IHubClients> _mockHubClients;

        public TaskControllerTests()
        {
            _mockContext = new Mock<TaskManagementContext>(new DbContextOptions<TaskManagementContext>());
            _mockPriorityService = new Mock<TaskPriorityService>();
            _mockHubContext = new Mock<IHubContext<TaskHub>>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockHubClients = new Mock<IHubClients>();
            
            _mockHubContext.Setup(x => x.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(x => x.All).Returns(_mockClientProxy.Object);
        }

        [Fact]
        public async Task CreateTask_ValidInput_ReturnsCreatedResult()
        {
            // Arrange
            var category = new Category { 
                Id = 1, 
                Name = "Test Category", 
                Priority = PriorityLevel.Medium 
            };
            var user = new User { 
                Id = 1, 
                Username = "testuser", 
                Email = "test@example.com", 
                Password = "testpass" 
            };
            var taskDto = new CreateTaskDTO
            {
                Title = "Test Task",
                Description = "Test Description",
                CategoryId = 1,
                UserId = 1,
                DueDate = DateTime.Now.AddDays(1),
                Priority = 3
            };

            var mockDbSetCategories = MockDbSet(new List<Category> { category });
            var mockDbSetUsers = MockDbSet(new List<User> { user });
            var mockDbSetTasks = MockDbSet(new List<TaskItem>());
            var mockDbSetSubscriptions = MockDbSet(new List<TaskSubscription>());

            _mockContext.Setup(c => c.Categories).Returns(mockDbSetCategories.Object);
            _mockContext.Setup(c => c.Users).Returns(mockDbSetUsers.Object);
            _mockContext.Setup(c => c.TaskItems).Returns(mockDbSetTasks.Object);
            _mockContext.Setup(c => c.TaskSubscriptions).Returns(mockDbSetSubscriptions.Object);
            
            _mockContext.Setup(c => c.Categories.FindAsync(1))
                .ReturnsAsync(category);
            _mockContext.Setup(c => c.Users.FindAsync(1))
                .ReturnsAsync(user);

            // Setup priority calculation
            _mockPriorityService.Setup(p => p.CalculateTaskPriority(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>()))
                .Returns(3);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);

            // Act
            var result = await controller.CreateTask(taskDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdAtActionResult.Value);
        }

        [Fact]
        public async Task GetTask_ExistingId_ReturnsTask()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                Priority = 1,
                CategoryId = 1,
                Category = new Category { 
                    Id = 1, 
                    Name = "Test Category", 
                    Priority = PriorityLevel.Low 
                },
                TaskSubscriptions = new List<TaskSubscription>()
            };

            var tasks = new List<TaskItem> { task };
            var mockDbSet = MockDbSet(tasks);
            _mockContext.Setup(c => c.TaskItems).Returns(mockDbSet.Object);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);

            // Act
            var result = await controller.GetTask(taskId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TaskResponseDTO>(okResult.Value);
            Assert.Equal(taskId, returnValue.Id);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);
            return dbSet;
        }
    }

    public class CategoryControllerTests
    {
        private readonly Mock<TaskManagementContext> _mockContext;

        public CategoryControllerTests()
        {
            _mockContext = new Mock<TaskManagementContext>(new DbContextOptions<TaskManagementContext>());
        }

        [Fact]
        public async Task CreateCategory_ValidInput_ReturnsCreatedResult()
        {
            // Arrange
            var categoryDto = new CreateCategoryDTO { 
                Name = "Test Category", 
                Priority = (int)PriorityLevel.Low 
            };
            var categories = new List<Category>();
            var mockDbSet = MockDbSet(categories);
            _mockContext.Setup(c => c.Categories).Returns(mockDbSet.Object);

            var controller = new CategoriesController(_mockContext.Object);

            // Act
            var result = await controller.CreateCategory(categoryDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<CategoryResponseDTO>(createdAtActionResult.Value);
            Assert.Equal(categoryDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetCategories_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { 
                    Id = 1, 
                    Name = "Category 1", 
                    Priority = PriorityLevel.Low,
                    Tasks = new List<TaskItem>() 
                },
                new Category { 
                    Id = 2, 
                    Name = "Category 2", 
                    Priority = PriorityLevel.High,
                    Tasks = new List<TaskItem>() 
                }
            };
            var mockDbSet = MockDbSet(categories);
            _mockContext.Setup(c => c.Categories).Returns(mockDbSet.Object);

            var controller = new CategoriesController(_mockContext.Object);

            // Act
            var result = await controller.GetCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<CategoryResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);
            return dbSet;
        }
    }

    public class UserControllerTests
    {
        private readonly Mock<TaskManagementContext> _mockContext;

        public UserControllerTests()
        {
            _mockContext = new Mock<TaskManagementContext>(new DbContextOptions<TaskManagementContext>());
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

            var users = new List<User>();
            var mockDbSet = MockDbSet(users);
            _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.Register(userDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<UserRegDTO>(createdAtActionResult.Value);
            Assert.Equal(userDto.Username, returnValue.Username);
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

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "password123",
                Email = "test@test.com",
                TaskSubscriptions = new List<TaskSubscription>()
            };

            var users = new List<User> { user };
            var mockDbSet = MockDbSet(users);
            _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserResponseDTO>(okResult.Value);
            Assert.Equal(user.Username, returnValue.Username);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);
            return dbSet;
        }
    }
}