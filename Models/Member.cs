namespace zuHause.Models
{
    public class Member
    {
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public byte Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsLandlord { get; set; }
        // ... 你可以加其他欄位，但這些就能測試了
    }
}
