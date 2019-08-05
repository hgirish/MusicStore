using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Controllers;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.Test
{
   public class ManageControllerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public ManageControllerTest()
        {
            var services = new ServiceCollection();
            var efServiceProvider = services.AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddOptions();
            services.AddDbContext<MusicStoreContext>(b => b.UseInMemoryDatabase("Scratch")
            .UseInternalServiceProvider(efServiceProvider));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MusicStoreContext>();

            services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .AddSessionStateTempDataProvider();


            services.AddSingleton<IAuthenticationService, NoOpAuth>();
            services.AddLogging();

            var context = new DefaultHttpContext();
            services.AddSingleton<IHttpContextAccessor>(
                new HttpContextAccessor { HttpContext = context });

            _serviceProvider = services.BuildServiceProvider();

        }
        [Fact]
        public async Task Index_ReturnsViewBagsMessagesExpected()
        {
            var userId = "TestUserA";
            var phone = "abcdefg";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.HomePhone, phone)
            };

            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var userManagerResult = await userManager.CreateAsync(
                new ApplicationUser
                {
                    Id = userId,
                    UserName = "Test",
                    TwoFactorEnabled = true,
                    PhoneNumber = phone
                }, "Pass@word1");
            Assert.True(userManagerResult.Succeeded);

            var signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;

            var schemeProvider = _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var controller = new ManageController(userManager,signInManager,schemeProvider);
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);

            Assert.Empty(controller.ViewBag.StatusMessage);

            Assert.NotNull(viewResult.ViewData);
            var model = Assert.IsType<IndexViewModel>( viewResult.ViewData.Model);
            Assert.True(model.TwoFactor);
            Assert.Equal(phone, model.PhoneNumber);
            Assert.True(model.HasPassword);
        }

    }

    internal class NoOpAuth : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }

        public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }
    }
}
