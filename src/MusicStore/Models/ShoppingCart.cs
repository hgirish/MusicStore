using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Models
{
    public class ShoppingCart
    {
        private readonly MusicStoreContext _dbContext;
        private readonly string _shoppingCartId;

        public ShoppingCart(MusicStoreContext dbContext, string shoppingCartId)
        {
            _dbContext = dbContext;
            _shoppingCartId = shoppingCartId;
        }

        public static ShoppingCart GetCart(MusicStoreContext db, HttpContext context)
            => GetCart(db, GetCartId(context));

        public static ShoppingCart GetCart(MusicStoreContext db, string cartId)
            => new ShoppingCart(db, cartId);

        public async Task AddToCart(Album album)
        {
            var cartItem = await _dbContext.CartItems.SingleOrDefaultAsync(
                c => c.CartId == _shoppingCartId
                && c.AlbumId == album.AlbumId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    AlbumId = album.AlbumId,
                    CartId = _shoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.UtcNow
                };
                _dbContext.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Count++;
            }
        }

        public int RemoveFromCart(int id)
        {
            var cartItem = _dbContext.CartItems.SingleOrDefault(
                cart => cart.CartId == _shoppingCartId &&
                cart.CartItemId == id);

            int itemCount = 0;
            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    _dbContext.CartItems.Remove(cartItem);
                }
            }
            return itemCount;
        }

        public async Task EmptyCart()
        {
            var cartItems = await _dbContext.CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .ToArrayAsync();
            _dbContext.CartItems.RemoveRange(cartItems);
        }

        public Task<List<CartItem>> GetCartItems()
        {
            return _dbContext.CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .Include(c => c.Album)
                .ToListAsync();
        }

        public Task<List<string>> GetCartAlbumTitles()
        {
            return _dbContext.CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .Select(c => c.Album.Title)
                .OrderBy(n => n)
                .ToListAsync();
        }

        public Task<int> GetCount()
        {
            return _dbContext.CartItems
                .Where(c => c.CartId == _shoppingCartId)
                .Select(c => c.Count)
                .SumAsync();
        }

        public Task<decimal> GetTotal()
        {
            return _dbContext.CartItems
                .Where(c => c.CartId == _shoppingCartId)
                .Select(c => c.Album.Price * c.Count)
                .SumAsync();
        }

        public async Task<int> CreateOrder(Order order)
        {
            decimal orderTotal = 0;
            var cartItems = await GetCartItems();
            foreach (var item in cartItems)
            {
                var album = await _dbContext.Albums.SingleAsync(a =>
                a.AlbumId == item.AlbumId);

                var orderDetail = new OrderDetail
                {
                    AlbumId = item.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = album.Price,
                    Quantity = item.Count
                };

                orderTotal += (item.Count * album.Price);

                _dbContext.OrderDetails.Add(orderDetail);
            }

            order.Total = orderTotal;

            await EmptyCart();

            return order.OrderId;

        }

        private static string GetCartId(HttpContext context)
        {
            var cartId = context.Session.GetString(AppConstants.SessionCartId);
            if (cartId == null)
            {
                cartId = Guid.NewGuid().ToString();
                context.Session.SetString("CartId", cartId);
            }
            return cartId;
        }
    }
}
