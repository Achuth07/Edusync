using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Edusync.Controllers;
using Edusync.Data;
using Edusync.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Edusync.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly Mock<DbSet<Course>> _mockCourseSet;
        private readonly Mock<ILogger<CoursesController>> _mockLogger;
        private readonly CoursesController _controller;

        public CoursesControllerTests()
        {
            // Initialize the mocks for DbSet and ILogger
            _mockCourseSet = new Mock<DbSet<Course>>();
            _mockLogger = new Mock<ILogger<CoursesController>>();

            // Initialize the context mock
            _mockContext = new Mock<SchoolManagementDbContext>();
            _mockContext.Setup(c => c.Courses).Returns(_mockCourseSet.Object);

            // Initialize the controller with the mock context and logger
            _controller = new CoursesController(_mockContext.Object, _mockLogger.Object);
        }

        // Test for Index method
        [Fact]
        public async Task Index_ReturnsViewResultWithCourses()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { Id = 1, Code = "M101", Name = "Mathematics", Credits = 3 },
                new Course { Id = 2, Code = "S101", Name = "Science", Credits = 4 }
            }.AsQueryable();

            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.Provider).Returns(courses.Provider);
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.Expression).Returns(courses.Expression);
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.ElementType).Returns(courses.ElementType);
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.GetEnumerator()).Returns(courses.GetEnumerator());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Course>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        // Test for Create GET method
        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        // Test for Create POST method (Valid Model)
        [Fact]
        public async Task Create_PostValidModel_ReturnsRedirectToIndex()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "M101", Name = "Mathematics", Credits = 3 };

            // Act
            var result = await _controller.Create(course);

            // Assert
            _mockCourseSet.Verify(c => c.Add(It.IsAny<Course>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for Edit GET method
        [Fact]
        public async Task Edit_ReturnsViewResult_WithCourse()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "M101", Name = "Mathematics", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Delete GET method
        [Fact]
        public async Task Delete_ReturnsViewResult_WithCourse()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "M101", Name = "Mathematics", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Delete POST method (DeleteConfirmed)
        [Fact]
        public async Task DeleteConfirmed_DeletesCourseAndRedirectsToIndex()
        {
            // Arrange
            var course = new Course { Id = 1, Code = "M101", Name = "Mathematics", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _mockCourseSet.Verify(c => c.Remove(course), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
