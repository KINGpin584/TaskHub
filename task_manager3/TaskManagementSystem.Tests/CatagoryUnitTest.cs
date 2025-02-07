using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using TaskManagementSystem.Core;

namespace TaskManagementSystem.Tests.Controllers
{
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

        [Fact]
        public async Task GetCategory_ExistingId_ReturnsCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Priority = PriorityLevel.Medium,
                Tasks = new List<TaskItem>()
            };

            var categories = new List<Category> { category };
            var mockDbSet = MockDbSet(categories);
            _mockContext.Setup(c => c.Categories).Returns(mockDbSet.Object);

            var controller = new CategoriesController(_mockContext.Object);

            // Act
            var result = await controller.GetCategory(categoryId);

            // Assert
            var returnValue = Assert.IsType<CategoryResponseDTO>(result.Value);
            Assert.Equal(categoryId, returnValue.Id);
            Assert.Equal(category.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetCategory_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999;
            var categories = new List<Category>();
            var mockDbSet = MockDbSet(categories);
            _mockContext.Setup(c => c.Categories).Returns(mockDbSet.Object);

            var controller = new CategoriesController(_mockContext.Object);

            // Act
            var result = await controller.GetCategory(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
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