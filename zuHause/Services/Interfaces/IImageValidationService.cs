using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Services.Interfaces
{
    /// <summary>
    /// 圖片驗證服務介面
    /// </summary>
    public interface IImageValidationService
    {
        /// <summary>
        /// 驗證檔案格式是否有效
        /// </summary>
        /// <param name="mimeType">MIME 類型</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>驗證結果</returns>
        ValidationResult ValidateFileFormat(string mimeType, string fileName);

        /// <summary>
        /// 驗證檔案大小是否在允許範圍內
        /// </summary>
        /// <param name="fileSizeBytes">檔案大小（位元組）</param>
        /// <param name="category">圖片分類</param>
        /// <returns>驗證結果</returns>
        ValidationResult ValidateFileSize(long fileSizeBytes, ImageCategory category);

        /// <summary>
        /// 驗證圖片尺寸是否符合要求
        /// </summary>
        /// <param name="width">寬度</param>
        /// <param name="height">高度</param>
        /// <param name="category">圖片分類</param>
        /// <returns>驗證結果</returns>
        ValidationResult ValidateImageDimensions(int width, int height, ImageCategory category);

        /// <summary>
        /// 驗證實體關聯是否有效
        /// </summary>
        /// <param name="entityType">實體類型</param>
        /// <param name="entityId">實體ID</param>
        /// <returns>驗證結果的非同步任務</returns>
        Task<ValidationResult> ValidateEntityRelationAsync(EntityType entityType, int entityId);

        /// <summary>
        /// 完整驗證圖片實體
        /// </summary>
        /// <param name="image">圖片實體</param>
        /// <returns>驗證結果的非同步任務</returns>
        Task<ValidationResult> ValidateImageAsync(Image image);
    }

    /// <summary>
    /// 驗證結果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否驗證成功
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 錯誤訊息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 建立成功的驗證結果
        /// </summary>
        /// <returns>成功的驗證結果</returns>
        public static ValidationResult Success() => new ValidationResult { IsValid = true };

        /// <summary>
        /// 建立失敗的驗證結果
        /// </summary>
        /// <param name="errors">錯誤訊息</param>
        /// <returns>失敗的驗證結果</returns>
        public static ValidationResult Failure(params string[] errors) => new ValidationResult 
        { 
            IsValid = false, 
            Errors = errors.ToList() 
        };

        /// <summary>
        /// 建立失敗的驗證結果
        /// </summary>
        /// <param name="errors">錯誤訊息列表</param>
        /// <returns>失敗的驗證結果</returns>
        public static ValidationResult Failure(IEnumerable<string> errors) => new ValidationResult 
        { 
            IsValid = false, 
            Errors = errors.ToList() 
        };
    }
}