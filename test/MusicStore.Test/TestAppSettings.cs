using Microsoft.Extensions.Options;
using MusicStore.Models;

namespace MusicStore.Test
{
    public class TestAppSettings : IOptions<AppSettings>
    {
        public readonly AppSettings _appSettings;
        public TestAppSettings(bool storeInCache = true)
        {
            _appSettings = new AppSettings
            {
                SiteTitle = "ASP.NET MVC Music Store",
                CacheDbResults = storeInCache
            };
        }
        public AppSettings Value => _appSettings;
    }
}
