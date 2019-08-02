using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Controllers;
using MusicStore.Models;
using Xunit;

namespace MusicStore.Test
{
    public class HomeControllerTest
    {
        [Fact]
        public void Error_ReturnsErrorView()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var controller = new HomeController();
            controller.ControllerContext.HttpContext = httpContext;


            // Act
            var result = controller.Error();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);
            Assert.NotNull(viewResult.ViewData.Model);
            var model = Assert.IsType<ErrorViewModel>(viewResult.ViewData.Model);
            Assert.Equal(httpContext.TraceIdentifier, model.RequestId);

        }
    }
}
