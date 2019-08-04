using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Components
{
    public class GenreMenuComponent : ViewComponent
    {
        private readonly MusicStoreContext _dbContext;

        public GenreMenuComponent(MusicStoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var genres = await _dbContext.Genres
                 .OrderByDescending(g => g.Albums.Sum(a =>
             a.OrderDetails.Sum(od => od.Quantity)))
                .Select(g => g.Name)
                .Take(9)
                .ToListAsync();

            return View(genres);
        }
    }
}
