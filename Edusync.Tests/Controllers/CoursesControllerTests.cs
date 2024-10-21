using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edusync.Controllers;
using Edusync.Data;
using Edusync.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edusync.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly Mock<DbSet<Course>> _mockCourseSet;
        private readonly CoursesController _controller;

        public CoursesControllerTests()
        {
            _mockCourseSet = new Mock<DbSet<Course>>();
            _mockContext = new Mock<SchoolManagementDbContext>();

            _mockContext.Setup(c => c.Courses).Returns(_mockCourseSet.Object);

            _controller = new CoursesController(_mockContext.Object);
        }

        // Test for Index method
        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfCourses()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 },
                new Course { Id = 2, Name = "Science", Code = "S101", Credits = 4 }
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

        // Test for Details method
        [Fact]
        public async Task Details_ReturnsViewResult_WithCourse()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Details method when course does not exist
        [Fact]
        public async Task Details_ReturnsNotFound_WhenCourseIsNull()
        {
            // Arrange
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync((Course)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test for Create POST method when model is valid
        [Fact]
        public async Task Create_PostValidModel_ReturnsRedirectToIndex()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };

            // Act
            var result = await _controller.Create(course);

            // Assert
            _mockCourseSet.Verify(c => c.Add(It.IsAny<Course>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for Create POST method when model is invalid
        [Fact]
        public async Task Create_PostInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Create(course);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(course.Id, model.Id);
        }

        // Test for Edit GET method
        [Fact]
        public async Task Edit_ReturnsViewResult_WithCourse()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Edit POST method when model is valid
        [Fact]
        public async Task Edit_PostValidModel_ReturnsRedirectToIndex()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };

            // Act
            var result = await _controller.Edit(1, course);

            // Assert
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for Edit POST method when model is invalid
        [Fact]
        public async Task Edit_PostInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Edit(1, course);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(course.Id, model.Id);
        }

        // Test for Delete GET method
        [Fact]
        public async Task Delete_ReturnsViewResult_WithCourse()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for DeleteConfirmed method
        [Fact]
        public async Task DeleteConfirmed_RemovesCourseAndRedirectsToIndex()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Mathematics", Code = "M101", Credits = 3 };
            _mockCourseSet.Setup(m => m.FindAsync(1)).ReturnsAsync(course);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _mockCourseSet.Verify(c => c.Remove(course), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for CourseExists method
        [Fact]
        public void CourseExists_ReturnsTrue_WhenCourseExists()
        {
            // Arrange
            _mockCourseSet.Setup(m => m.Any(e => e.Id == 1)).Returns(true);

            // Act
            var result = _controller.CourseExists(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CourseExists_ReturnsFalse_WhenCourseDoesNotExist()
        {
            // Arrange
            _mockCourseSet.Setup(m => m.Any(e => e.Id == 1)).Returns(false);

            // Act
            var result = _controller.CourseExists(1);

            // Assert
            Assert.False(result);
        }
    }
}
