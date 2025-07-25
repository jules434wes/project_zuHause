using zuHause.Models;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 臨時會話管理服務介面
    /// 提供 GUID + Cookie 穩定會話機制，支援表單合併提交的臨時圖片管理
    /// </summary>
    public interface ITempSessionService
    {
        /// <summary>
        /// 取得或建立臨時會話ID
        /// 從 Cookie 讀取現有ID，若不存在則生成新的32字元 GUID 並設定 Cookie
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>32字元的臨時會話ID</returns>
        string GetOrCreateTempSessionId(HttpContext context);

        /// <summary>
        /// 驗證臨時會話ID是否有效
        /// 檢查 Memory Cache 中是否存在對應的會話資料
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <returns>是否有效</returns>
        Task<bool> IsValidTempSessionAsync(string tempSessionId);

        /// <summary>
        /// 失效臨時會話
        /// 清除 Memory Cache 中的會話資料，但保留 Cookie（讓瀏覽器自然過期）
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        Task InvalidateTempSessionAsync(string tempSessionId);

        /// <summary>
        /// 取得臨時會話中的圖片資訊列表
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <returns>圖片資訊列表，若無資料則回傳空列表</returns>
        Task<List<TempImageInfo>> GetTempImagesAsync(string tempSessionId);

        /// <summary>
        /// 新增圖片到臨時會話
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <param name="tempImageInfo">臨時圖片資訊</param>
        Task AddTempImageAsync(string tempSessionId, TempImageInfo tempImageInfo);

        /// <summary>
        /// 從臨時會話中移除圖片
        /// </summary>
        /// <param name="tempSessionId">臨時會話ID</param>
        /// <param name="imageGuid">圖片GUID</param>
        /// <returns>是否成功移除</returns>
        Task<bool> RemoveTempImageAsync(string tempSessionId, Guid imageGuid);

        /// <summary>
        /// 清理過期的臨時會話
        /// 移除所有超過6小時的臨時會話資料
        /// </summary>
        /// <returns>清理的會話數量</returns>
        Task<int> CleanupExpiredSessionsAsync();
    }
}