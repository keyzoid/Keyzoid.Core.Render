namespace Keyzoid.Core.Render.Models
{
    public abstract class Content
    {
        public int createdOn { get; set; }
        public int? modifiedOn { get; set; }
        public bool? isActive { get; set; }
    }
}
