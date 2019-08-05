using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private const string PromoCode = "FREE";

        public IActionResult AddressAndPayment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressAndPayment(
            [FromServices] MusicStoreContext dbContext,
            [FromForm] Order order,
            CancellationToken requestAborted)
        {
            if (!ModelState.IsValid)
            {
                return View(order);
            }
            var formCollection = await HttpContext.Request.ReadFormAsync();

            try
            {
                if (string.Equals(formCollection["PromoCode"].FirstOrDefault(),PromoCode,
                    StringComparison.OrdinalIgnoreCase) == false)
                {
                    return View(order);
                }
                else
                {
                    order.Username = HttpContext.User.Identity.Name;
                    order.OrderDate = DateTime.UtcNow;

                    dbContext.Orders.Add(order);
                    string cartId = HttpContext.Session.GetString(AppConstants.SessionCartId);
                    var cart = ShoppingCart.GetCart(dbContext, cartId);
                    await cart.CreateOrder(order);

                    await dbContext.SaveChangesAsync(requestAborted);

                    return RedirectToAction("Complete", new { id = order.OrderId });
                }
            }
            catch 
            {
                return View(order);
            }
        }

        public async Task<IActionResult> Complete(
            [FromServices] MusicStoreContext dbContext, int id)
        {
            var userName = HttpContext.User.Identity.Name;

            bool isValid = await dbContext.Orders.AnyAsync(
                o => o.OrderId == id &&
                o.Username == userName);

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }

        }
    }
}
