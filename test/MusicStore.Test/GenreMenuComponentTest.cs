using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Components;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.Test
{
  public  class GenreMenuComponentTest
    {
        private readonly IServiceProvider _serviceProvider;

        public GenreMenuComponentTest()
        {
            var services = new ServiceCollection();
            var efServiceProvider = services.AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            services.AddDbContext<MusicStoreContext>(b =>
            b.UseInMemoryDatabase("Scratch")
            .UseInternalServiceProvider(efServiceProvider));

            _serviceProvider = services.BuildServiceProvider();
        }
        [Fact]
        public async Task GenreMenuComponent_Returns_NineGenres()
        {
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var genreMenuComponent = new GenreMenuComponent(dbContext);

            PopulateData(dbContext);

            var result = await genreMenuComponent.InvokeAsync();

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.Null(viewResult.ViewName);
            var genreResult = Assert.IsType<List<string>>(viewResult.ViewData.Model);
            Assert.Equal(9, genreResult.Count);
        }

        private void PopulateData(MusicStoreContext dbContext)
        {
            var genres = Enumerable.Range(1, 10)
                .Select(n => new Genre
                {
                    GenreId = n
                });
            dbContext.AddRange(genres);
            dbContext.SaveChanges();
        }
    }
}
