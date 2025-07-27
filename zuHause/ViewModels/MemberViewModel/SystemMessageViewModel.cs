namespace zuHause.ViewModels.MemberViewModel
{
    public class SystemMessageViewModel
    {
        /// <summary>
        /// 來源類型 (User / System)
        /// </summary>
        public string SourceType { get; set; } = "User";
        //User 使用者
        //System 系統

        /// <summary>
        /// 通知ID
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// 接受者類型代碼
        /// </summary>
        public string AudienceTypeCode { get; set; } = null!;


        /// <summary>
        /// 通知類型代碼
        /// </summary>
        public string TypeCode { get; set; } = null!;

        /// <summary>
        /// 訊息標題
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// 訊息內容
        /// </summary>
        public string NotificationContent { get; set; } = null!;

        /// <summary>
        /// 相關連結URL
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// 來源模組代碼
        /// </summary>
        public string? ModuleCode { get; set; }

        /// <summary>
        /// 來源資料ID
        /// </summary>
        public int? SourceEntityId { get; set; }

        /// <summary>
        /// 是否已讀
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// 閱讀時間
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// 發送時間
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// 刪除時間
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 關聯圖片
        /// </summary>
        public List<string> ImageUrls { get; set; } = new();

        public string? ApplicationStatus { get; set; }

    }
}
