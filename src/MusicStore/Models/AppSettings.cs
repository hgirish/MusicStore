using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Models
{
    public class AppSettings
    {
        public string SiteTitle { get; set; }

        public bool CacheDbResults { get; set; } = true;
    }
}
