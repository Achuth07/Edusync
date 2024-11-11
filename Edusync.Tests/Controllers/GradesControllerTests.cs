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
    public class GradesControllerTests
    {
        private readonly Mock<SchoolManagementDbContext> _mockContext;
        private readonly Mock<DbSet<Grade>> _mockGradeSet;
        private readonly Mock<ILogger<GradesController>> _mockLogger;
        private readonly GradesController _controller;

        public GradesControllerTests()
        {
            // Initialize the mocks for DbSet and ILogger
            _mockGradeSet = new Mock<DbSet<Grade>>();
            _mockLogger = new Mock<ILogger<GradesController>>();
            
            // Initialize the context mock
            _mockContext = new Mock<SchoolManagementDbContext>();
            _mockContext.Setup(c => c.Grades).Returns(_mockGradeSet.Object);

            // Initialize the controller with the mock context and logger
            _controller = new GradesController(_mockContext.Object, _mockLogger.Object);
        }

        // Test for Index method
        [Fact]
        public async Task Index_ReturnsViewResultWithGrades()
        {
            // Arrange
            var grades = new List<Grade>
            {
                new Grade { Id = 1, AssessmentType = "Midterm", Score = 85 },
                new Grade { Id = 2, AssessmentType = "Final", Score = 90 }
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
            var grade = new Grade { Id = 1, AssessmentType = "Midterm", Score = 85 };

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
            var grade = new Grade { Id = 1, AssessmentType = "Midterm", Score = 85 };
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
            var grade = new Grade { Id = 1, AssessmentType = "Midterm", Score = 85 };
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
            var grade = new Grade { Id = 1, AssessmentType = "Midterm", Score = 85 };
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
