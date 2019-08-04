using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using MusicStore.Controllers;
using MusicStore.Models;
using MusicStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.Test
{
   public class ShoppingCartControllerTest
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ShoppingCartController _controller;
        readonly MusicStoreContext _dbContext;
        readonly string _cartId;


        public ShoppingCartControllerTest()
        {
            _cartId = "CartId_A";
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var services = new ServiceCollection();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddDbContext<MusicStoreContext>(b =>
                b.UseInMemoryDatabase("Scratch")
                .UseInternalServiceProvider(efServiceProvider));
    
            services.AddMvcCore();
           

            _serviceProvider = services.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var httpContext = new DefaultHttpContext
            {
                Session = new TestSession()
            };
            _controller = new ShoppingCartController(_dbContext);
            _controller.ControllerContext.HttpContext = httpContext;



        }

        [Fact]
        public async Task Index_ReturnsNoCartItems_WhenSessionEmpty()
        {
           // var controller = new ShoppingCartController(null);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);

            var model = Assert.IsType<ShoppingCartViewModel>(viewResult.ViewData.Model);
            Assert.Empty(model.CartItems);
            Assert.Equal(0, model.CartTotal);
        }

        [Fact]
        public async Task Index_ReturnsNoCartItems_WhenNoItemsInCart()
        {
            //var controller = new ShoppingCartController(null);
            _controller.HttpContext.Session.SetString(AppConstants.SessionCartId, _cartId);
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);

            var model = Assert.IsType<ShoppingCartViewModel>(viewResult.ViewData.Model);
            Assert.Empty(model.CartItems);
            Assert.Equal(0, model.CartTotal);
        }

        [Fact]
        public async Task Index_ReturnsCartItems_WhenItemsInCart()
        {
            
            AddTestCartItems(_cartId);

            _controller.HttpContext.Session.SetString(AppConstants.SessionCartId, _cartId);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);

            var model = Assert.IsType<ShoppingCartViewModel>(viewResult.ViewData.Model);
            Assert.Equal(5, model.CartItems.Count);
            Assert.Equal(5 * 10, model.CartTotal);

        }

        [Fact]
        public async Task AddToCart_AddsItemToCartAsync()
        {
            var albumId = 3;
            AddTestAblums();
          
            _controller.HttpContext.Session.SetString(AppConstants.SessionCartId, _cartId);

            var result = await _controller.AddToCart(albumId, CancellationToken.None);
           // var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var sessionCartId = _controller.HttpContext.Session.GetString(AppConstants.SessionCartId);
            var cart = ShoppingCart.GetCart(_dbContext, sessionCartId);

            List<CartItem> cartItems = await cart.GetCartItems();
            Assert.Single(cartItems);
            Assert.Equal(albumId, cartItems.Single().AlbumId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);

           


        }

        private void AddTestCartItems(string cartId)
        {
           // var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var cartItems = CreateTestCartItems(cartId,
                itemPrice: 10,
                numberOfItems: 5);
            _dbContext.AddRange(cartItems.Select(n => n.Album).Distinct());
            _dbContext.AddRange(cartItems);
            _dbContext.SaveChanges();
        }

        private void AddTestAblums()
        {
           // var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var albums = CreateTestAlbums(itemPrice: 10);
            _dbContext.AddRange(albums);
            _dbContext.SaveChanges();
        }

        private static CartItem[] CreateTestCartItems(string cartId, decimal itemPrice, int numberOfItems)
        {
            Album[] albums = CreateTestAlbums(itemPrice);

            var cartItems = Enumerable.Range(1, numberOfItems)
                .Select(n => new CartItem
                {
                    Count = 1,
                    CartId = cartId,
                    AlbumId = n % albums.Length,
                    Album = albums[n % albums.Length],
                }).ToArray();
            return cartItems;
        }

        private static Album[] CreateTestAlbums(decimal itemPrice)
        {
            return Enumerable.Range(1, 10)
                .Select(n => new Album
                {
                    AlbumId = n,
                    Price = itemPrice
                }).ToArray();
        }
    }
}
