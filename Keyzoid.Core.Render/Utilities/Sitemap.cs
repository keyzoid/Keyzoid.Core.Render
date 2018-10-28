using Keyzoid.Core.Render.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using static Keyzoid.Core.Render.Models.Enums;

namespace Keyzoid.Core.Render.Utilities
{
    /// <summary>
    /// Generates a site map.
    /// </summary>
    public class SitemapGenerator : BaseSitemapGenerator
    {
        private const string BlogPath = @"C:\PATH\blogs.json";
        private const string AlbumPath = @"C:\PATH\albums.json";
        private const string PlaylistPath = @"C:\PATH\playlists.json";
        private const string TagsPath = @"C:\PATH\tags.json";

        protected override void GenerateUrlNodes(SiteContent content = null)
        {
            WriteUrlLocation("sitemap.xml", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("blog", UpdateFrequency.Daily, DateTime.Now);
            WriteBlogs(content?.Blogs);
            WriteUrlLocation("blog/search", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("albums", UpdateFrequency.Weekly, DateTime.Now);
            WriteAlbums(content?.PhotoAlbums);
            WriteUrlLocation("music", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("music/search", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("music/playlists", UpdateFrequency.Weekly, DateTime.Now);
            WritePlaylists(content?.Playlists);
            WriteUrlLocation("music/tabs", UpdateFrequency.Weekly, DateTime.Now);
            WriteBookmarks(content?.Tabs);
            WriteUrlLocation("projects", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("art", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("contact", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("code", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("bookmarks", UpdateFrequency.Weekly, DateTime.Now);
            WriteBookmarks(content?.Bookmarks);
            WriteUrlLocation("bookmarks/search", UpdateFrequency.Weekly, DateTime.Now);
            WriteUrlLocation("tags", UpdateFrequency.Weekly, DateTime.Now);
            WriteTags(content?.Tags);
            WriteUrlLocation("store", UpdateFrequency.Weekly, DateTime.Now);
        }

        private void WriteBlogs(Blog[] blogs = null)
        {
            if (blogs == null)
            {
                using (var reader = new StreamReader(BlogPath))
                {
                    var blogsRaw = reader.ReadToEnd();
                    blogs = JsonConvert.DeserializeObject<Blog[]>(blogsRaw);
                }
            }

            foreach (var blog in blogs)
            {
                WriteUrlLocation($"blog/{blog.uniqueName}", UpdateFrequency.Weekly, DateTime.Now);
            }
        }

        private void WriteAlbums(PhotoAlbum[] albums = null)
        {
            if (albums == null)
            {
                using (var reader = new StreamReader(AlbumPath))
                {
                    var albumsRaw = reader.ReadToEnd();
                    albums = JsonConvert.DeserializeObject<PhotoAlbum[]>(albumsRaw);
                }
            }

            foreach (var album in albums)
            {
                WriteUrlLocation($"albums/{album.uniqueName}", UpdateFrequency.Weekly, DateTime.Now);

                foreach (var picture in album.pictures)
                {
                    var image = Path.GetFileNameWithoutExtension(picture.image);
                    WriteUrlLocation($"albums/{album.uniqueName}/{image}", UpdateFrequency.Weekly, DateTime.Now);
                }
            }
        }

        private void WritePlaylists(Playlist[] playlists = null)
        {
            if (playlists == null)
            {
                using (var reader = new StreamReader(PlaylistPath))
                {
                    var playlistsRaw = reader.ReadToEnd();
                    playlists = JsonConvert.DeserializeObject<Playlist[]>(playlistsRaw);
                }
            }

            foreach (var playlist in playlists)
            {
                WriteUrlLocation($"music/playlists/{playlist.uniqueName}", UpdateFrequency.Weekly, DateTime.Now);
            }
        }

        private void WriteTags(Tag[] tags = null)
        {
            if (tags == null)
            {
                using (var reader = new StreamReader(TagsPath))
                {
                    var tagsRaw = reader.ReadToEnd();
                    tags = JsonConvert.DeserializeObject<Tag[]>(tagsRaw);
                }
            }

            foreach (var tag in tags)
            {
                WriteUrlLocation($"tags/{tag.uniqueName}", UpdateFrequency.Weekly, DateTime.Now);
            }
        }

        private void WriteBookmarks(Bookmark[] bookmarks = null)
        {
            if (bookmarks == null)
            {
                using (var reader = new StreamReader(TagsPath))
                {
                    var bmarksRaw = reader.ReadToEnd();
                    bookmarks = JsonConvert.DeserializeObject<Bookmark[]>(bmarksRaw);
                }
            }

            foreach (var bookmark in bookmarks)
            {
                WriteUrlLocation($"bookmarks/{bookmark.createdOn}", UpdateFrequency.Weekly, DateTime.Now);
            }
        }
    }

    /// <summary>
    /// Generates an index site map for Keyzoid.Web.Simple.
    /// </summary>
    public class SitemapIndexGenerator : BaseSitemapIndexGenerator
    {
        protected override void GenerateUrlNodes()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Sitemap class that creates a web site map for Keyzoid.Web.Simple.
    /// </summary>
    public class Sitemap
    {
        #region Fields

        private const string OutputPath = @"C:\USER_PATH\Desktop";

        #endregion

        #region Constructors

        /// <summary>
        /// Main entry point to the application.
        /// </summary>
        /// <param name="args">Arguments to pass into the application.</param>
        public static void Execute(string[] args)
        {
            var siteMapGenerator = new SitemapGenerator();
            var siteMap1 = siteMapGenerator.Generate("http://www.YOUR-URL.com");

            using (var writer = new StreamWriter($"{OutputPath}/sitemap.xml"))
            {
                writer.WriteLine(siteMap1);
            }
        }

        #endregion
    }
}
