namespace Keyzoid.Core.Render.Models
{
    public class Picture
    {
        public string caption { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public bool? isPortrait { get; set; }
        public string[] tags { get; set; }
    }
}
