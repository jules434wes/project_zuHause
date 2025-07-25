namespace zuHause.Models
{
    /// <summary>
    /// 連線測試結果
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TestedAt { get; set; } = DateTime.UtcNow;

        public static ConnectionTestResult Successful() 
            => new() { Success = true, Message = "連線測試成功" };

        public static ConnectionTestResult Failure(string message) 
            => new() { Success = false, Message = message };
    }
}