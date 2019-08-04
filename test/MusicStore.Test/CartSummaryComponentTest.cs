using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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
  public   class CartSummaryComponentTest
    {
        private readonly IServiceProvider _serviceProvider;
        public CartSummaryComponentTest()
        {
            var services = new ServiceCollection();
            var efServiceProvider = services
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<MusicStoreContext>(b =>
            b.UseInMemoryDatabase("Scratch")
            .UseInternalServiceProvider(efServiceProvider));

            _serviceProvider = services.BuildServiceProvider();

           

        }
        [Fact]
        public async Task CartSummaryComponent_Returns_CartedItems()
        {
            var viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext(),

            };

            var cartId = "CartId_A";
            viewContext.HttpContext.Session = new TestSession();
            viewContext.HttpContext.Session.SetString(AppConstants.SessionCartId, cartId);

            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            PopulateData(dbContext, cartId: cartId, albumTitle: "AlbumA", itemCount: 10);
            var cartSummaryComponent = new CartSummaryComponent(dbContext)
            {
                ViewComponentContext = new ViewComponentContext
                {
                    ViewContext = viewContext
                }
            };

            var result = await cartSummaryComponent.InvokeAsync();

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model);
            Assert.Equal(10, cartSummaryComponent.ViewBag.CartCount);
            Assert.Equal("AlbumA", cartSummaryComponent.ViewBag.CartSummary);

        }

        private void PopulateData(MusicStoreContext dbContext, string cartId, string albumTitle, int itemCount)
        {
            var album = new Album
            {
                AlbumId = 1,
                Title = albumTitle
            };
            var cartItems = Enumerable.Range(1, itemCount)
                .Select(n => new CartItem
                {
                    AlbumId = 1,
                    Album = album,
                    Count = 1,
                    CartId = cartId
                }).ToArray();
            dbContext.AddRange(cartItems);
            dbContext.SaveChanges();
        }
    }
}
