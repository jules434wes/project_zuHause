namespace zuHause.ViewModels
{
    public class MarqueeUpdateViewModel
    {
        public int SiteMessagesId { get; set; }
        public string SiteMessageContent { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public bool IsActive { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
