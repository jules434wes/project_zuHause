namespace zuHause.ViewModels.MemberViewModel
{
    public class ApplicationRecordViewModel
    {
        public int ApplicationId { get; set; }
        public string? ApplicationType { get; set; }
        public List<ApplicationStatusLogViewModel> StatusLogs { get; set; } = new();

        public int PropertyId { get; set; }
        public int LandlordMemberId { get; set; } // 傳訊用
        public string Title { get; set; } = null!; // 房源標題
        public string? AddressLine { get; set; } // 房源地址
        public decimal MonthlyRent { get; set; } // 房源租金
        public int? ContractId { get; set; } // 合約ID，如果走到簽約流程了就會有

        /// <summary>
        /// 圖片位址
        /// </summary>
        public string? imgPath { get; set; }

        public DateTime? ScheduleTime { get; set; }
        public DateOnly? RentalStartDate { get; set; }
        public DateOnly? RentalEndDate { get; set; }

        public DateTime CreatedAt { get; set; } // 申請時間
    }

    public class ApplicationStatusLogViewModel
    {
        public int StatusLogId { get; set; } // 狀態歷程ID
        public string StatusCode { get; set; } = null!; // 狀態代碼
        public DateTime ChangedAt { get; set; } // 進入狀態時間
    }

    public class MemberApplicationCardInputViewModel
    {
        public ApplicationRecordViewModel Record { get; set; } = null!;
        public Dictionary<string, string> StatusCodeDict { get; set; } = new();

    }
}
