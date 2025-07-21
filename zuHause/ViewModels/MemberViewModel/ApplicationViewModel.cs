namespace zuHause.ViewModels.MemberViewModel
{
    public class ApplicationViewModel
    {
        public string? ApplicationType { get; set; }

        public int PropertyId { get; set; }

        public string? Message { get; set; }

        public DateOnly? ScheduleDate { get; set; }


        public DateTime? ScheduleTime
        {
            get
            {
                if (ScheduleDate == null || string.IsNullOrEmpty(ScheduleHouse) || string.IsNullOrEmpty(ScheduleMinute))
                {
                    return null;
                }

                try
                {
                    string dateString = ScheduleDate.Value.ToString("yyyy-MM-dd");
                    string timeString = $"{ScheduleHouse}:{ScheduleMinute}";

                    return DateTime.ParseExact($"{dateString} {timeString}", "yyyy-MM-dd HH:mm", null);
                }
                catch
                {
                    return null; 
                }
            }
        }


        public string? ScheduleHouse { get; set; }
        public string? ScheduleMinute { get; set; }

        public DateOnly? RentalStartDate { get; set; }

        public DateOnly? RentalEndDate { get; set; }

        public string? ReturnUrl { get; set; }

    }
}
