namespace Keyzoid.Core.Render.Models
{
    public class SiteContent : DataResponse
    {
        public Blog[] BlogViews { get; set; }

        public PhotoAlbum[] PhotoAlbumViews { get; set; }

        public Playlist[] PlaylistViews { get; set; }
    }
}
