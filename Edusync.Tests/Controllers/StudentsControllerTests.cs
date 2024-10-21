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
    public class StudentsControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            // Mock the DbContext
            _mockContext = new Mock<SchoolManagementDbContext>(new DbContextOptions<SchoolManagementDbContext>());
            
            // Initialize the StudentsController with the mock DbContext
            _controller = new StudentsController(_mockContext.Object);
        }

        // Helper method to mock DbSet<T>
        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var dbSetMock = new Mock<DbSet<T>>();
            
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return dbSetMock;
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
            }.AsQueryable();  // Convert list to IQueryable to simulate EF Core behavior

            // Mock the DbSet using the helper method
            var mockSet = CreateDbSetMock(students);

            // Setup the context to return the mocked DbSet
            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        // Test for Details method - when student exists
        [Fact]
        public async Task Details_ReturnsViewWithStudent()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };
            var students = new List<Student> { student }.AsQueryable();
            var mockSet = CreateDbSetMock(students);

            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Student>(viewResult.ViewData.Model);
            Assert.Equal(student.Id, model.Id);
        }

        // Test for Details method - when student does not exist
        [Fact]
        public async Task Details_ReturnsNotFoundWhenStudentNotFound()
        {
            // Arrange
            var students = new List<Student>().AsQueryable();
            var mockSet = CreateDbSetMock(students);

            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
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

        // Test for Create method (POST)
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

        // Test for Edit method (GET) - when student exists
        [Fact]
        public async Task Edit_GetReturnsViewWithStudent()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };
            var students = new List<Student> { student }.AsQueryable();
            var mockSet = CreateDbSetMock(students);

            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);
            _mockContext.Setup(db => db.Students.FindAsync(1)).ReturnsAsync(student);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Student>(viewResult.ViewData.Model);
            Assert.Equal(student.Id, model.Id);
        }

        // Test for Edit method (POST)
        [Fact]
        public async Task Edit_PostValidModel_RedirectsToIndex()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };

            _mockContext.Setup(db => db.Update(student));

            // Act
            var result = await _controller.Edit(1, student);

            // Assert
            _mockContext.Verify(db => db.Update(student), Times.Once);
            _mockContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for Delete method (GET) - when student exists
        [Fact]
        public async Task Delete_GetReturnsViewWithStudent()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };
            var students = new List<Student> { student }.AsQueryable();
            var mockSet = CreateDbSetMock(students);

            _mockContext.Setup(db => db.Students).Returns(mockSet.Object);
            _mockContext.Setup(db => db.Students.FindAsync(1)).ReturnsAsync(student);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Student>(viewResult.ViewData.Model);
            Assert.Equal(student.Id, model.Id);
        }

        // Test for Delete method (POST)
        [Fact]
        public async Task DeleteConfirmed_DeletesStudentAndRedirectsToIndex()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe" };
            var students = new List<Student> { student }.AsQueryable();
            var mockSet = CreateDbSetMock(students);

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
