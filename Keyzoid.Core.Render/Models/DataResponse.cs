namespace Keyzoid.Core.Render.Models
{
    public class DataResponse
    {
        public Tag[] Tags { get; set; }

        public Blog[] Blogs { get; set; }

        public PhotoAlbum[] PhotoAlbums { get; set; }

        public Playlist[] Playlists { get; set; }

        public Bookmark[] Bookmarks { get; set; }

        public Bookmark[] Tabs { get; set; }

        public Feature[] Features { get; set; }

        public ContentCount[] Counts { get; set; }
    }
}
