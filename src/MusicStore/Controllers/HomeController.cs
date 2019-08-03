using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class HomeController : Controller
    {
        public readonly AppSettings _appSettings;
        public HomeController(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
        }
        public async Task<IActionResult> Index(
            [FromServices]MusicStoreContext dbContext,
            [FromServices] IMemoryCache cache)
        {
            var cacheKey = "topselling";

            List<Album> albums;
            if (!cache.TryGetValue(cacheKey, out albums))
            {
                albums = await GetTopSellingAlbumsAsync(dbContext, 6);
                if (albums != null && albums.Count > 0)
                {
                    // Cache albums
                    if (_appSettings.CacheDbResults)
                    {
                        cache.Set(
                            cacheKey,
                            albums,
                            new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                            .SetPriority(CacheItemPriority.High));
                    }
                } 
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
