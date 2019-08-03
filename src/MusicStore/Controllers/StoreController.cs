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
    public class StoreController : Controller
    {
        private readonly MusicStoreContext _dbContext;
        private readonly AppSettings _appSettings;

        public StoreController(MusicStoreContext dbContext, IOptions<AppSettings> options)
        {
            _dbContext = dbContext;
            _appSettings = options.Value;
        }

        public async Task<IActionResult> Index()
        {
            var genres = await _dbContext.Genres.ToListAsync();
            return View(genres);
        }

        public async  Task<IActionResult> Browse(string genere)
        {
            var genreModel = await _dbContext.Genres
                .Include(g => g.Albums)
                .Where(g => g.Name == genere)
                .FirstOrDefaultAsync();

            if (genreModel == null)
            {
                return NotFound();
            }
            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache, int id)
        {
            var cacheKey = $"album_{id}";
            Album album;
            if (!cache.TryGetValue(cacheKey, out album))
            {
                album = await _dbContext.Albums
                    .Where(a => a.AlbumId == id)
                    .Include(a => a.Artist)
                    .Include(a => a.Genre)
                    .FirstOrDefaultAsync();

                if (album != null)
                {
                    if (_appSettings.CacheDbResults)
                    {
                        cache.Set(
                            cacheKey,
                            album,
                            new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                    }
                }

            }

            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }
    }
}
