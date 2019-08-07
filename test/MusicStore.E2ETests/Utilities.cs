using System;
using System.Linq;
using MusicStore.Models;

namespace MusicStore.E2ETests
{
    public class Utilities
    {
        public static void InitializeDbForTests(MusicStoreContext db)
        {
            db.Albums.AddRange(GetAlbums());
            db.SaveChanges();
        }

        public static Album[] GetAlbums()
        {
            var generes = Enumerable.Range(1, 10).Select(n =>
            new Genre
            {
                GenreId = n,
                Name = "Genre Name " + n
            }).ToArray();

            var artists = Enumerable.Range(1, 10).Select(n =>
             new Artist
             {
                 ArtistId = n + 1,
                 Name = "Artist Name " + n,
             }).ToArray();

            var albums = Enumerable.Range(1, 10).Select(n =>
             new Album
             {
                 Artist = artists[n - 1],
                 ArtistId = n,
                 Genre = generes[n - 1],
                 GenreId = n
             }).ToArray();
            return albums;
        }
    }
}