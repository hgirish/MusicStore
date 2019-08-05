using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MusicStore.Test.Models
{
   public class ShoppingCartTest : IClassFixture<ShoppingCartFixture>
    {
        private readonly ShoppingCartFixture _fixture;

        public ShoppingCartTest(ShoppingCartFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async System.Threading.Tasks.Task ComputesTotalAsync()
        {
            var cartId = Guid.NewGuid().ToString();

            using (var db = _fixture.CreateContext())
            {
                var a = db.Albums.Add(new Album
                {
                    Price = 15.99m
                }).Entity;

                db.CartItems.Add(
                    new CartItem
                    {
                        Album = a,
                        Count = 2,
                        CartId = cartId
                    });

                db.SaveChanges();

                Assert.Equal(31.98m,
                    await ShoppingCart.GetCart(db, cartId).GetTotal());

            }
        }
    }

    public class ShoppingCartFixture
    {
        private readonly IServiceProvider _serviceProvider;

        public ShoppingCartFixture()
        {
            var services = new ServiceCollection();

            var efServiceProvider = services.AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<MusicStoreContext>(b =>
            b.UseInMemoryDatabase("Scratch")
            .UseInternalServiceProvider(efServiceProvider));

            _serviceProvider = services.BuildServiceProvider();
        }

        public virtual MusicStoreContext CreateContext() =>
            _serviceProvider.GetRequiredService<MusicStoreContext>();
    }
}
