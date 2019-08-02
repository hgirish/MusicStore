using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(
            [FromServices]MusicStoreContext dbContext)
        {
            List<Album> albums;
            albums = await GetTopSellingAlbumsAsync(dbContext, 6);
            if (albums != null && albums.Count > 0)
            {
                // Cache albums
            }
            return View(albums);
        }

        private async Task<List<Album>> GetTopSellingAlbumsAsync(MusicStoreContext dbContext, int count)
        {
            return (await dbContext.Albums
                .OrderByDescending(a => a.OrderDetails.Count)
                .Take(count)
                .ToListAsync());
        }

        public IActionResult Privacy()
        {
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult StatusCodePage()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
