namespace Keyzoid.Core.Render.Models
{
    public partial class Blog : Content
    {
        public string uniqueName { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string author { get; set; }
        public int? likes { get; set; }
        public string image { get; set; }
        public string[] tags { get; set; }
    }
}
