namespace Keyzoid.Core.Render.Models
{
    public partial class Bookmark : Content
    {
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string category { get; set; }
        public bool isTab { get; set; }
        public Video video { get; set; }
        public string[] tags { get; set; }
    }
}
