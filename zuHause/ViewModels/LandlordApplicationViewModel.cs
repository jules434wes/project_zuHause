namespace zuHause.ViewModels
{
    /// <summary>
    /// 房東申請頁面的 ViewModel
    /// </summary>
    public class LandlordApplicationViewModel
    {
        /// <summary>
        /// 會員姓名
        /// </summary>
        public string MemberName { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已經是房東身份
        /// </summary>
        public bool IsAlreadyLandlord { get; set; }
        
        /// <summary>
        /// 是否可以申請成為房東
        /// </summary>
        public bool CanApply { get; set; }
        
        /// <summary>
        /// 錯誤訊息（當不符合申請資格時）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}