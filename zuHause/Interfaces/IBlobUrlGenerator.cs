using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Blob Storage URL 生成服務介面
    /// 提供統一的圖片 URL 生成邏輯，支援臨時區域和正式區域
    /// </summary>
    public interface IBlobUrlGenerator
    {
        /// <summary>
        /// 生成正式區域的圖片 URL
        /// </summary>
        /// <param name="category">圖片分類</param>
        /// <param name="entityId">實體 ID</param>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>完整的圖片 URL</returns>
        string GenerateImageUrl(ImageCategory category, int entityId, Guid imageGuid, ImageSize size);

        /// <summary>
        /// 生成臨時區域的圖片 URL
        /// </summary>
        /// <param name="tempSessionId">臨時會話 ID（32字元）</param>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>臨時區域的完整圖片 URL</returns>
        string GenerateTempImageUrl(string tempSessionId, Guid imageGuid, ImageSize size);

        /// <summary>
        /// 取得 Blob 路徑（不含基礎 URL）
        /// 用於 Azure Blob Storage 操作
        /// </summary>
        /// <param name="category">圖片分類</param>
        /// <param name="entityId">實體 ID</param>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>Blob 路徑</returns>
        string GetBlobPath(ImageCategory category, int entityId, Guid imageGuid, ImageSize size);

        /// <summary>
        /// 取得臨時 Blob 路徑（不含基礎 URL）
        /// 用於 Azure Blob Storage 操作
        /// </summary>
        /// <param name="tempSessionId">臨時會話 ID</param>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="size">圖片尺寸</param>
        /// <returns>臨時 Blob 路徑</returns>
        string GetTempBlobPath(string tempSessionId, Guid imageGuid, ImageSize size);

        /// <summary>
        /// 驗證 URL 格式是否正確
        /// </summary>
        /// <param name="url">要驗證的 URL</param>
        /// <returns>是否為有效的圖片 URL</returns>
        bool IsValidImageUrl(string url);
    }
}