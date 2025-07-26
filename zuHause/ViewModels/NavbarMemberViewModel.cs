using System.ComponentModel.DataAnnotations;

namespace zuHause.ViewModels
{
    /// <summary>
    /// 導航欄會員資訊 ViewModel
    /// 支援角色感知的動態導航顯示
    /// </summary>
    public class NavbarMemberViewModel
    {
        /// <summary>
        /// 用戶頭像路徑
        /// </summary>
        public string Avatar { get; set; } = "~/images/user-image.jpg";
        
        /// <summary>
        /// 是否為房東身份
        /// </summary>
        public bool IsLandlord { get; set; }
        
        /// <summary>
        /// 會員類型ID (1: 房客, 2: 房東)
        /// </summary>
        public int? MemberTypeId { get; set; }
        
        /// <summary>
        /// 是否已完成身份驗證
        /// </summary>
        public bool IsIdentityVerified { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 是否已登入
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// 應顯示的導航類型 (landlord: 房東導航, tenant: 房客導航, guest: 訪客導航)
        /// </summary>
        public string NavigationType => GetNavigationType();
        
        private string GetNavigationType()
        {
            if (!IsAuthenticated) return "guest";
            if (IsLandlord && MemberTypeId == 2 && IsIdentityVerified) return "landlord";
            return "tenant";
        }
    }
}