using zuHause.Models;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Azure Blob Storage 連線測試介面
    /// </summary>
    public interface IBlobStorageConnectionTest
    {
        /// <summary>
        /// 測試 Azure Blob Storage 連線
        /// </summary>
        /// <returns>連線測試結果</returns>
        Task<ConnectionTestResult> TestConnectionAsync();
    }
}