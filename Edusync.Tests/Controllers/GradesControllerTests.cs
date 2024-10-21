using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Controllers;
using Edusync.Data;
using Edusync.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edusync.Tests.Controllers
{
    public class GradesControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly Mock<DbSet<Grade>> _mockGradeSet;
        private readonly Mock<DbSet<Student>> _mockStudentSet;
        private readonly Mock<DbSet<Class>> _mockClassSet;
        private readonly GradesController _controller;

        public GradesControllerTests()
        {
            // Initialize the mocks for the DbSets
            _mockGradeSet = new Mock<DbSet<Grade>>();
            _mockStudentSet = new Mock<DbSet<Student>>();
            _mockClassSet = new Mock<DbSet<Class>>();

            // Initialize the context mock
            _mockContext = new Mock<SchoolManagementDbContext>();
            _mockContext.Setup(c => c.Grades).Returns(_mockGradeSet.Object);
            _mockContext.Setup(c => c.Students).Returns(_mockStudentSet.Object);
            _mockContext.Setup(c => c.Classes).Returns(_mockClassSet.Object);

            // Initialize the controller with the mock context
            _controller = new GradesController(_mockContext.Object);
        }

        // Test for Index method
        [Fact]
        public async Task Index_ReturnsViewResultWithGrades()
        {
            // Arrange
            var grades = new List<Grade>
            {
                new Grade { Id = 1, AssessmentType = "Midterm", Score = 85, AcademicYear = "2024", Student = new Student { FirstName = "John", LastName = "Doe" }, Class = new Class { Course = new Course { Code = "M101", Name = "Mathematics" } } },
                new Grade { Id = 2, AssessmentType = "Final", Score = 90, AcademicYear = "2024", Student = new Student { FirstName = "Jane", LastName = "Smith" }, Class = new Class { Course = new Course { Code = "S101", Name = "Science" } } }
            }.AsQueryable();

            _mockGradeSet.As<IQueryable<Grade>>().Setup(m => m.Provider).Returns(grades.Provider);
            _mockGradeSet.As<IQueryable<Grade>>().Setup(m => m.Expression).Returns(grades.Expression);
            _mockGradeSet.As<IQueryable<Grade>>().Setup(m => m.ElementType).Returns(grades.ElementType);
            _mockGradeSet.As<IQueryable<Grade>>().Setup(m => m.GetEnumerator()).Returns(grades.GetEnumerator());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Grade>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        // Test for Create GET method
        [Fact]
        public void Create_ReturnsViewResult_WithSelectLists()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, FirstName = "John", LastName = "Doe" },
                new Student { Id = 2, FirstName = "Jane", LastName = "Smith" }
            }.AsQueryable();

            var classes = new List<Class>
            {
                new Class { Id = 1, Course = new Course { Code = "M101", Name = "Mathematics" } },
                new Class { Id = 2, Course = new Course { Code = "S101", Name = "Science" } }
            }.AsQueryable();

            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.Provider).Returns(students.Provider);
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.Expression).Returns(students.Expression);
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.ElementType).Returns(students.ElementType);
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.GetEnumerator()).Returns(students.GetEnumerator());

            _mockClassSet.As<IQueryable<Class>>().Setup(m => m.Provider).Returns(classes.Provider);
            _mockClassSet.As<IQueryable<Class>>().Setup(m => m.Expression).Returns(classes.Expression);
            _mockClassSet.As<IQueryable<Class>>().Setup(m => m.ElementType).Returns(classes.ElementType);
            _mockClassSet.As<IQueryable<Class>>().Setup(m => m.GetEnumerator()).Returns(classes.GetEnumerator());

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<SelectList>(viewResult.ViewData["StudentId"]);
            Assert.IsType<SelectList>(viewResult.ViewData["ClassId"]);
        }

        // Test for Create POST method (Valid Model)
        [Fact]
        public async Task Create_PostValidModel_ReturnsRedirectToIndex()
        {
            // Arrange
            var grade = new Grade { Id = 1, StudentId = 1, ClassId = 1, AssessmentType = "Midterm", Score = 85, AcademicYear = "2024" };

            // Act
            var result = await _controller.Create(grade);

            // Assert
            _mockGradeSet.Verify(g => g.Add(It.IsAny<Grade>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Test for Edit GET method
        [Fact]
        public async Task Edit_ReturnsViewResult_WithGrade()
        {
            // Arrange
            var grade = new Grade { Id = 1, StudentId = 1, ClassId = 1, AssessmentType = "Midterm", Score = 85, AcademicYear = "2024" };
            _mockGradeSet.Setup(m => m.FindAsync(1)).ReturnsAsync(grade);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Grade>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Delete GET method
        [Fact]
        public async Task Delete_ReturnsViewResult_WithGrade()
        {
            // Arrange
            var grade = new Grade { Id = 1, StudentId = 1, ClassId = 1, AssessmentType = "Midterm", Score = 85, AcademicYear = "2024" };
            _mockGradeSet.Setup(m => m.FindAsync(1)).ReturnsAsync(grade);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Grade>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        // Test for Delete POST method (DeleteConfirmed)
        [Fact]
        public async Task DeleteConfirmed_DeletesGradeAndRedirectsToIndex()
        {
            // Arrange
            var grade = new Grade { Id = 1, StudentId = 1, ClassId = 1, AssessmentType = "Midterm", Score = 85, AcademicYear = "2024" };
            _mockGradeSet.Setup(m => m.FindAsync(1)).ReturnsAsync(grade);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _mockGradeSet.Verify(g => g.Remove(grade), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
