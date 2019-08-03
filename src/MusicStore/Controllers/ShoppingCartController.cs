using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Models;
using MusicStore.ViewModels;

namespace MusicStore.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly MusicStoreContext _dbContext;

        public ShoppingCartController(MusicStoreContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var cartId = HttpContext.Session.GetString(AppConstants.SessionCartId);
           
            var cart = ShoppingCart.GetCart(_dbContext, cartId);

            var viewModel = new ShoppingCartViewModel {
                CartItems = await cart.GetCartItems(),
                CartTotal = await cart.GetTotal()
            };
            return View(viewModel);
        }
    }
}