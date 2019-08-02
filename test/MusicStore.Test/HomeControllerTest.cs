using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Controllers;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.Test
{
    public class HomeControllerTest
    {
        private  IServiceProvider _serviceProvider;
        public HomeControllerTest()
        {
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            var services = new ServiceCollection();

            services.AddDbContext<MusicStoreContext>(b =>
            b.UseInMemoryDatabase("Scratch")
            .UseInternalServiceProvider(efServiceProvider));

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task HomePage_ReturnsIndexViewAsync()
        {
            


            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            var controller = new HomeController();
            PopulateData(dbContext);
            var result = await controller.Index(dbContext);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData.Model);
            var albums = Assert.IsType<List<Album>>(viewResult.ViewData.Model);
            Assert.Equal(6, albums.Count);
        }

        private void PopulateData(MusicStoreContext dbContext)
        {
            var albums = TestAlbumDataProvider.GetAlbums();

            foreach (var album in albums)
            {
                dbContext.Add(album);
            }
            dbContext.SaveChanges();
        }

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
        [Fact]
        public void StatusCodePage_ReturnsStatusCodePage()
        {
            var controller = new HomeController();

            var result = controller.StatusCodePage();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model);
        }
        [Fact]
        public void AccessDenied_ReturnsAccessDeniedView()
        {
            var controller = new HomeController();

            var result = controller.AccessDenied();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model);
        }

        private class TestAlbumDataProvider
        {
            public static Album[] GetAlbums()
            {
                var generes = Enumerable.Range(1, 10).Select(n =>
                new Genre
                {
                    GenreId = n,
                    Name = "Genre Name " + n
                }).ToArray();

                var artists = Enumerable.Range(1, 10).Select(n =>
                 new Artist
                 {
                     ArtistId = n + 1,
                     Name = "Artist Name " + n,
                 }).ToArray();

                var albums = Enumerable.Range(1, 10).Select(n =>
                 new Album
                 {
                     Artist = artists[n - 1],
                     ArtistId = n,
                     Genre = generes[n - 1],
                     GenreId = n
                 }).ToArray();
                return albums;
            }
        }
    }
}
