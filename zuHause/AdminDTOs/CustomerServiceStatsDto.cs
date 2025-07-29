namespace zuHause.AdminDTOs
{
    public class CustomerServiceStatsDto
    {
        public int PendingCount { get; set; }
        public int ProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int TotalCount { get; set; }
        public int UnresolvedCount => PendingCount + ProgressCount;
    }
}
