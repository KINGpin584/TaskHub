using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Controllers;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Core;
using Microsoft.AspNetCore.SignalR;
using TaskManagementSystem.API.Hubs;
using TaskManagementSystem.API.Services;

namespace TaskManagementSystem.Tests.Controllers
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
        public async Task GetTasks_WithFilters_ReturnsFilteredTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task 1",
                    Description = "Description 1",
                    DueDate = DateTime.Now.AddDays(1),
                    Status = TaskState.Incomplete,
                    Priority = 3,
                    CreatedAt = DateTime.Now,
                    CategoryId = 1,
                    Category = new Category { Id = 1, Name = "Category 1", Priority = PriorityLevel.Medium },
                    TaskSubscriptions = new List<TaskSubscription>()
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task 2",
                    Description = "Description 2",
                    DueDate = DateTime.Now.AddDays(2),
                    Status = TaskState.Completed,
                    Priority = 1,
                    CreatedAt = DateTime.Now,
                    CategoryId = 2,
                    Category = new Category { Id = 2, Name = "Category 2", Priority = PriorityLevel.Low },
                    TaskSubscriptions = new List<TaskSubscription>()
                }
            };

            var mockDbSet = MockDbSet(tasks);
            _mockContext.Setup(c => c.TaskItems).Returns(mockDbSet.Object);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);

            // Act
            var result = await controller.GetTasks(
                state: TaskState.Incomplete,
                priority: 3,
                categoryId: 1,
                includeCompleted: false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTasks = Assert.IsType<List<TaskResponseDTO>>(okResult.Value);
            Assert.Single(returnedTasks);
            Assert.Equal("Task 1", returnedTasks[0].Title);
        }

        [Fact]
        public async Task UpdateTaskState_CompletedTask_SetsCompletedAtDate()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                Priority = 1,
                CategoryId = 1
            };

            _mockContext.Setup(c => c.TaskItems.FindAsync(taskId))
                .ReturnsAsync(task);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);
            var updateDto = new UpdateTaskStateDTO { State = TaskState.Completed };

            // Act
            var result = await controller.UpdateTaskState(taskId, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(TaskState.Completed, task.Status);
            Assert.NotNull(task.CompletedAt);
        }

        [Fact]
        public async Task DeleteTask_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                Priority = 1,
                CategoryId = 1
            };

            _mockContext.Setup(c => c.TaskItems.FindAsync(taskId))
                .ReturnsAsync(task);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);

            // Act
            var result = await controller.DeleteTask(taskId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockContext.Verify(c => c.TaskItems.Remove(task), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Original Title",
                Description = "Original Description",
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskState.Incomplete,
                CreatedAt = DateTime.Now,
                Priority = 1,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Category", Priority = PriorityLevel.Low }
            };

            _mockContext.Setup(c => c.TaskItems.FindAsync(taskId))
                .ReturnsAsync(task);

            var updateDto = new UpdateTaskDTO
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Priority = 2
            };

            _mockPriorityService.Setup(p => p.CalculateTaskPriority(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>()))
                .Returns(2);

            var controller = new TasksController(_mockContext.Object, _mockPriorityService.Object, _mockHubContext.Object);

            // Act
            var result = await controller.UpdateTask(taskId, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal("Updated Title", task.Title);
            Assert.Equal("Updated Description", task.Description);
            Assert.Equal(2, task.Priority);
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