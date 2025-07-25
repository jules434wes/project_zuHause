using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// Blob Storage 操作服務介面
    /// 提供併發控制和自動重試機制的 Blob 操作功能
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// 單一檔案上傳（包含重試機制）
        /// </summary>
        /// <param name="stream">檔案串流</param>
        /// <param name="blobPath">Blob 路徑</param>
        /// <param name="contentType">內容類型（預設為 image/webp）</param>
        /// <param name="overwrite">是否覆蓋已存在檔案（預設為 true）</param>
        /// <returns>上傳結果</returns>
        Task<BlobUploadResult> UploadWithRetryAsync(
            Stream stream, 
            string blobPath, 
            string contentType = "image/webp", 
            bool overwrite = true);

        /// <summary>
        /// 原子性多尺寸上傳
        /// 所有尺寸必須全部上傳成功，否則回滾已上傳的檔案
        /// </summary>
        /// <param name="streams">各尺寸的檔案串流</param>
        /// <param name="basePath">基礎路徑（不含尺寸部分）</param>
        /// <param name="contentType">內容類型（預設為 image/webp）</param>
        /// <returns>原子性上傳結果</returns>
        Task<AtomicUploadResult> UploadMultipleSizesAsync(
            Dictionary<ImageSize, Stream> streams, 
            string basePath, 
            string contentType = "image/webp");

        /// <summary>
        /// 刪除 Blob 檔案
        /// </summary>
        /// <param name="blobPath">Blob 路徑</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteAsync(string blobPath);

        /// <summary>
        /// 刪除多個 Blob 檔案
        /// </summary>
        /// <param name="blobPaths">Blob 路徑列表</param>
        /// <returns>刪除結果（路徑 -> 是否成功）</returns>
        Task<Dictionary<string, bool>> DeleteMultipleAsync(IEnumerable<string> blobPaths);

        /// <summary>
        /// 檢查 Blob 是否存在
        /// </summary>
        /// <param name="blobPath">Blob 路徑</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string blobPath);

        /// <summary>
        /// 取得 Blob 的中繼資料
        /// </summary>
        /// <param name="blobPath">Blob 路徑</param>
        /// <returns>Blob 資訊（若不存在則為 null）</returns>
        Task<BlobMetadata?> GetBlobInfoAsync(string blobPath);

        /// <summary>
        /// 移動 Blob 檔案（從臨時區域到正式區域）
        /// </summary>
        /// <param name="sourcePath">來源路徑</param>
        /// <param name="destinationPath">目標路徑</param>
        /// <param name="deleteSource">是否刪除來源檔案（預設為 true）</param>
        /// <returns>移動結果</returns>
        Task<BlobUploadResult> MoveAsync(string sourcePath, string destinationPath, bool deleteSource = true);

        /// <summary>
        /// 批次移動 Blob 檔案
        /// </summary>
        /// <param name="moveOperations">移動操作清單（來源路徑 -> 目標路徑）</param>
        /// <param name="deleteSource">是否刪除來源檔案（預設為 true）</param>
        /// <returns>批次移動結果</returns>
        Task<Dictionary<string, BlobUploadResult>> MoveBatchAsync(
            Dictionary<string, string> moveOperations, 
            bool deleteSource = true);

        /// <summary>
        /// 清理過期的臨時檔案
        /// </summary>
        /// <param name="olderThanHours">清理幾小時前的檔案（預設為 6 小時）</param>
        /// <returns>清理的檔案數量</returns>
        Task<int> CleanupExpiredTempFilesAsync(int olderThanHours = 6);
    }

    /// <summary>
    /// Blob 中繼資料模型
    /// </summary>
    public class BlobMetadata
    {
        /// <summary>
        /// Blob 路徑
        /// </summary>
        public string BlobPath { get; set; } = string.Empty;

        /// <summary>
        /// Blob URL
        /// </summary>
        public string BlobUrl { get; set; } = string.Empty;

        /// <summary>
        /// 檔案大小（位元組）
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// 內容類型
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// ETag
        /// </summary>
        public string ETag { get; set; } = string.Empty;

        /// <summary>
        /// 是否為臨時檔案
        /// </summary>
        public bool IsTemporary => BlobPath.StartsWith("temp/");

        /// <summary>
        /// 是否已過期（針對臨時檔案）
        /// </summary>
        /// <param name="expirationHours">過期時間（小時）</param>
        /// <returns>是否已過期</returns>
        public bool IsExpired(int expirationHours = 6)
        {
            if (!IsTemporary) return false;
            return DateTime.UtcNow - CreatedOn.UtcDateTime > TimeSpan.FromHours(expirationHours);
        }
    }
}