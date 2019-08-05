using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using MusicStore.Controllers;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.Test
{
  public  class CheckoutControllerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public CheckoutControllerTest()
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
        public void AddressAndPayment_ReturnsDefaultView()
        {
            var controller = new CheckoutController();

            var result = controller.AddressAndPayment();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task AddressAndPayment_RedirectToCompletedWhenSuccessful()
        {
            var httpContext = new DefaultHttpContext();

            var orderId = 10;

            var order = new Order
            {
                OrderId = orderId,
            };

            var cartId = "CartId_A";
            httpContext.Session = new TestSession();
            httpContext.Session.SetString(AppConstants.SessionCartId, cartId);

            httpContext.Request.Form =
                new FormCollection(
                    new Dictionary<string, StringValues>()
                    {
                        {"PromoCode",new[]{"FREE"} }
                    });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,"TestUserName" )
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            var cartItems = CreateTestCartItems(cartId, 
                itemPrice: 10.00m,
                numberOfItem: 1
                );
            var albums = cartItems.Select(n => n.Album).Distinct();

            dbContext.AddRange(albums);
            dbContext.AddRange(cartItems);
            dbContext.SaveChanges();

            var controller = new CheckoutController();
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.AddressAndPayment(dbContext, order, CancellationToken.None);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Complete", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName);
            Assert.NotNull(redirectResult.RouteValues);

            Assert.Equal(orderId, redirectResult.RouteValues["Id"]);
        }

        [Fact]
        public async Task AddressAndPayment_ReturnsOrderIfInvalidPromoCode()
        {
            var context = new DefaultHttpContext();
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            context.Request.Form =
                new FormCollection(
                    new Dictionary<string, StringValues>());

            var controller = new CheckoutController();
            controller.ControllerContext.HttpContext = context;

            var order = new Order();

            var result = await controller.AddressAndPayment(dbContext, order, CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);

            Assert.NotNull(viewResult.ViewData);
            Assert.Same(order, viewResult.ViewData.Model);
        }

        [Fact]
        public async Task AddressAndPayment_ReturnsOrderIfRequestCancelled()
        {
            var context = new DefaultHttpContext();
            context.Request.Form =
                new FormCollection(
                    new Dictionary<string, StringValues>());
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            var controller = new CheckoutController();
            controller.ControllerContext.HttpContext = context;

            var order = new Order();

            var result = await controller.AddressAndPayment(dbContext, order, new CancellationToken(true));

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);

            Assert.NotNull(viewResult.ViewData);
            Assert.Same(order, viewResult.ViewData.Model);
        }

        [Fact]
        public async Task AddressAndPayment_ReturnsOrderIfInvalidOrderModel()
        {
            var controller = new CheckoutController();
            controller.ModelState.AddModelError("a", "ModelErrorA");
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            var order = new Order();

            var result = await controller.AddressAndPayment(dbContext, order, CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);

            Assert.NotNull(viewResult.ViewData);
            Assert.Same(order, viewResult.ViewData.Model);
            var modelState = viewResult.ViewData.ModelState;
            Assert.NotNull(modelState);
            Assert.Equal(1, modelState.ErrorCount);
            var key = modelState.Keys.FirstOrDefault();
            Assert.Equal("a", key);
            var firstError = modelState.Values.FirstOrDefault().Errors.FirstOrDefault();
            Assert.Equal("ModelErrorA",firstError.ErrorMessage);
            foreach (var ms in modelState.Values)
            {
                foreach (var error in ms.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
        }

        public async Task Complete_ReturnsOrderIdIfValid()
        {
            var orderId = 100;
            var userName = "TestUserA";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userName)
            };

            var httpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            };

            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();

            dbContext.Add(new Order { OrderId = orderId, Username = userName });
            dbContext.SaveChanges();

            var controller = new CheckoutController();
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.Complete(dbContext, orderId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);
            Assert.Equal(orderId, viewResult.ViewData.Model);

        }

        [Fact]
        public async    Task Complete_ReturnsErrorIfInvlaidOrder()
        {
            var invalidOrderId = 100;
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            var controller = new CheckoutController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = await controller.Complete(dbContext, invalidOrderId);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Error", viewResult.ViewName);
        }

        private CartItem[] CreateTestCartItems(string cartId, decimal itemPrice, int numberOfItem)
        {
            var albums = Enumerable.Range(1, 10).Select(n =>
                new Album()
                {
                    AlbumId = n,
                    Price = itemPrice,
                }).ToArray();

            var cartItems = Enumerable.Range(1, numberOfItem).Select(n =>
                new CartItem()
                {
                    Count = 1,
                    CartId = cartId,
                    AlbumId = n % 10,
                    Album = albums[n % 10],
                }).ToArray();

            return cartItems;
        }
    }
}
