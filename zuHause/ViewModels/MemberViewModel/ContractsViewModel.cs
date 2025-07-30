namespace zuHause.ViewModels.MemberViewModel
{
    public class ContractsViewModel
    {
        public int ApplicationId { get;set;}
        public string? ApplicationType { get;set;}
        public int MemberId {get;set;}
        public int PropertyId {get;set;}
        public string? CurrentStatus {get;set;}
        public int? ContractId {get;set;}
        public DateOnly? StartDate {get;set;}
        public DateOnly? EndDate {get;set;}

        public string? StartDateText => StartDate?.ToString("yyyy-MM-dd");
        public string? EndDateText => EndDate?.ToString("yyyy-MM-dd");

        public string? Status { get;set;}
        public string? HaveFurniture {get;set;}
        public string? CustomName { get;set;}
        public int LandlordMemberId { get;set;}
        public DateTime? PublishedAt { get; set; }

        public string? PublishedAtText => PublishedAt?.ToString("yyyy-MM-dd");

        public string? Title { get;set;}
        public string? AddressLine { get;set;}
        public decimal MonthlyRent { get;set;}
        public string? PreviewImageUrl { get;set;}
        public string? StatusDisplayName { get; set; }


        public string? ApplicantName { get; set; }
        public DateOnly? ApplicantBath { get; set; }

        public string? ApplicantBathText => ApplicantBath?.ToString("yyyy-MM-dd");

        /// <summary>
        /// 圖片位址
        /// </summary>
        public string? imgPath { get; set; }


        public string DurationText
        {
            get
            {
                if (StartDate == null || EndDate == null) return "無效日期";

                var start = StartDate.Value.ToDateTime(TimeOnly.MinValue);
                var end = EndDate.Value.ToDateTime(TimeOnly.MinValue);

                int years = end.Year - start.Year;
                int months = end.Month - start.Month;
                int days = end.Day - start.Day;

                if (days < 0)
                {
                    months -= 1;
                    days += DateTime.DaysInMonth(end.AddMonths(-1).Year, end.AddMonths(-1).Month);
                }

                if (months < 0)
                {
                    years -= 1;
                    months += 12;
                }

                return $"{years}年{months}月{days}日";
            }
        }




    }



}
