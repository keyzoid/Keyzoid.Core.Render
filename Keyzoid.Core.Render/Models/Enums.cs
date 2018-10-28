namespace Keyzoid.Core.Render.Models
{
    public static class Enums
    {
        public enum UpdateFrequency
        {
            Always,
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Yearly,
            Never
        }

        public enum ContentType
        {
            Blog,
            Bookmark,
            Tab,
            Message,
            PhotoAlbum,
            Playlist,
            Tag,
            BlogView,
            PhotoAlbumView,
            PlaylistView
        }

        public enum TableContent
        {
            ContentType,
            CreatedOn,
            ModifiedOn,
            Name,
            Title,
            Description,
            Content,
            Author,
            Feature,
            Thumbnail,
            Tags,
            Video,
            IsActive
        }
    }
}
