namespace zuHause.ViewModels
{
    public class MarqueeCreateViewModel
    {
        public string SiteMessageContent { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public bool IsActive { get; set; }
        public string ModuleScope { get; set; } = null!;
        public string Category { get; set; } = "MARQUEE";
        public string? AttachmentUrl { get; set; }

    }
}
