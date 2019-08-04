using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Components
{
    [ViewComponent(Name ="CartSummary")]
    public class CartSummaryComponent : ViewComponent
    {
        private readonly MusicStoreContext _dbContext;

        public CartSummaryComponent(MusicStoreContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartId = HttpContext.Session.GetString(AppConstants.SessionCartId);
            var cart = ShoppingCart.GetCart(_dbContext, cartId);

            var cartItems = await cart.GetCartItems();
            ViewBag.CartCount = cartItems.Sum(c => c.Count);
            ViewBag.CartSummary = string.Join("\n", 
                cartItems.Select(c => c.Album.Title).Distinct());

            return View();
        }
    }
}
