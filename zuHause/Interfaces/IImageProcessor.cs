using zuHause.DTOs;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 圖片處理器介面 - 提供純技術的圖片處理能力
    /// 設計原則：小而聚焦、職責單一、測試友善
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// 將圖片轉換為 WebP 格式
        /// </summary>
        /// <param name="sourceStream">來源圖片串流</param>
        /// <param name="maxWidth">最大寬度 (可選，null 表示不限制)</param>
        /// <param name="quality">品質 (1-100，預設 80)</param>
        /// <returns>處理結果，包含轉換後的圖片串流和元數據</returns>
        Task<ImageProcessingResult> ConvertToWebPAsync(
            Stream sourceStream, 
            int? maxWidth = null, 
            int quality = 80);

        /// <summary>
        /// 生成指定尺寸的縮圖
        /// </summary>
        /// <param name="sourceStream">來源圖片串流</param>
        /// <param name="width">目標寬度</param>
        /// <param name="height">目標高度</param>
        /// <returns>處理結果，包含縮圖串流和元數據</returns>
        Task<ImageProcessingResult> GenerateThumbnailAsync(
            Stream sourceStream, 
            int width, 
            int height);

        /// <summary>
        /// 處理多尺寸圖片 - 針對臨時區域上傳使用
        /// 生成 4 種尺寸：Original, Large, Medium, Thumbnail
        /// </summary>
        /// <param name="sourceStream">來源圖片串流</param>
        /// <param name="imageGuid">圖片 GUID</param>
        /// <param name="originalFileName">原始檔案名稱</param>
        /// <param name="quality">WebP 品質 (1-100，預設 90)</param>
        /// <returns>多尺寸處理結果</returns>
        Task<MultiSizeProcessingResult> ProcessMultipleSizesAsync(
            Stream sourceStream, 
            Guid imageGuid,
            string originalFileName,
            int quality = 90);
    }
}