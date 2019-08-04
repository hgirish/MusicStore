using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> AddToCart(int id, CancellationToken requestAborted)
        {
            var cartId = HttpContext.Session.GetString(AppConstants.SessionCartId);
            var addedAlbum = await _dbContext.Albums
                .SingleAsync(album => album.AlbumId == id);

            var cart = ShoppingCart.GetCart(_dbContext, cartId);

            await cart.AddToCart(addedAlbum);
            await _dbContext.SaveChangesAsync(requestAborted);

            return RedirectToAction("Index");
        }
    }
}