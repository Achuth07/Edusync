using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Edusync.Controllers;
using Edusync.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Edusync.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            // Mock the ILogger<HomeController> dependency
            _mockLogger = new Mock<ILogger<HomeController>>();

            // Initialize the HomeController with the mocked logger
            _controller = new HomeController(_mockLogger.Object);
        }

        // Test for Index method
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);  // Ensure the default view is returned
        }

        // Test for About method
        [Fact]
        public void About_ReturnsViewResult()
        {
            // Act
            var result = _controller.About();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);  // Ensure the default view is returned
        }

        // Test for Privacy method
        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);  // Ensure the default view is returned
        }

        // Test for Error method
        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            var expectedRequestId = Activity.Current?.Id ?? "test-trace-id";

            // Simulate HttpContext and TraceIdentifier
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = expectedRequestId;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(viewResult.ViewData.Model);
            Assert.Equal(expectedRequestId, model.RequestId);
        }
    }
}
