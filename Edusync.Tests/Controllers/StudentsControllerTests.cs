using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edusync.Controllers;
using Edusync.Data;
using Edusync.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edusync.Tests.Controllers
{
    public class StudentsControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly Mock<ILogger<StudentsController>> _mockLogger;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            // Mock the DbContext and Logger
            _mockContext = new Mock<SchoolManagementDbContext>(new DbContextOptions<SchoolManagementDbContext>());
            _mockLogger = new Mock<ILogger<StudentsController>>();

            // Initialize the StudentsController with the mock DbContext and mock Logger
            _controller = new StudentsController(_mockContext.Object, _mockLogger.Object);
        }

        // Test for Index method
        [Fact]
        public async Task Index_ReturnsViewWithListOfStudents()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, FirstName = "John", LastName = "Doe" },
                new Student { Id = 2, FirstName = "Jane", LastName = "Smith" }
            }.AsQueryable();

            // Mock the DbSet
            var mockSet = new Mock<DbSet<Student>>();
            mockSet.As<IQueryable<Student>>().Setup(m => m.Provider).Returns(students.Provider);
            mockSet.As<IQueryable<Student>>().Setup(m => m.Expression).Returns(students.Expression);
            mockSet.As<IQueryable<Student>>().Setup(m => m.ElementType).Returns(students.ElementType);
            mockSet.As<IQueryable<Student>>().Setup(m => m.GetEnumerator()).Returns(students.GetEnumerator());

            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        // Test for Create method (GET)
        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // Test for Create method (POST) - valid model
        [Fact]
        public async Task Create_PostValidModel_RedirectsToIndex()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };

            // Act
            var result = await _controller.Create(student);

            // Assert
            _mockContext.Verify(db => db.Add(student), Times.Once);
            _mockContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for DeleteConfirmed method (POST) - valid ID
        [Fact]
        public async Task DeleteConfirmed_DeletesStudentAndRedirectsToIndex()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };

            _mockContext.Setup(db => db.Students.FindAsync(1)).ReturnsAsync(student);
            _mockContext.Setup(db => db.Students.Remove(student));

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _mockContext.Verify(db => db.Students.Remove(student), Times.Once);
            _mockContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
