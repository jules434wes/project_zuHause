using Microsoft.AspNetCore.Http;
using zuHause.DTOs;
using zuHause.Models;
using zuHause.Enums;

namespace zuHause.Interfaces
{
    /// <summary>
    /// 房源圖片服務介面 - Facade 模式接口
    /// 提供房源專用的圖片管理功能，簡化常用操作
    /// </summary>
    public interface IPropertyImageService
    {
        /// <summary>
        /// 批量上傳房源圖片
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">圖片檔案集合</param>
        /// <returns>上傳結果清單</returns>
        Task<List<PropertyImageResult>> UploadPropertyImagesAsync(int propertyId, IFormFileCollection files);

        /// <summary>
        /// 批量上傳房源圖片，指定分類
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">圖片檔案集合</param>
        /// <param name="category">圖片分類</param>
        /// <returns>上傳結果清單</returns>
        Task<List<PropertyImageResult>> UploadPropertyImagesByCategoryAsync(int propertyId, IFormFileCollection files, ImageCategory category);

        /// <summary>
        /// 批量上傳房源圖片，使用中文分類（自動轉換為英文儲存）
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="files">圖片檔案集合</param>
        /// <param name="chineseCategory">中文分類名稱</param>
        /// <returns>上傳結果清單</returns>
        Task<List<PropertyImageResult>> UploadPropertyImagesByChineseCategoryAsync(int propertyId, IFormFileCollection files, string chineseCategory);

        /// <summary>
        /// 取得房源主圖
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>主圖實體，沒有主圖時返回 null</returns>
        Task<Image?> GetMainPropertyImageAsync(int propertyId);

        /// <summary>
        /// 取得房源的所有圖片
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>圖片實體清單</returns>
        Task<List<Image>> GetPropertyImagesAsync(int propertyId);

        /// <summary>
        /// 取得房源圖片數量
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>圖片數量</returns>
        Task<int> GetPropertyImageCountAsync(int propertyId);

        /// <summary>
        /// 檢查房源是否有圖片
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>是否有圖片</returns>
        Task<bool> HasPropertyImagesAsync(int propertyId);

        /// <summary>
        /// 刪除房源圖片
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeletePropertyImageAsync(long imageId);

        /// <summary>
        /// 刪除房源的所有圖片
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteAllPropertyImagesAsync(int propertyId);

        /// <summary>
        /// 設定房源主圖
        /// </summary>
        /// <param name="imageId">圖片ID</param>
        /// <returns>是否設定成功</returns>
        Task<bool> SetMainPropertyImageAsync(long imageId);

        /// <summary>
        /// 重新排序房源圖片
        /// </summary>
        /// <param name="propertyId">房源ID</param>
        /// <param name="imageIds">重新排序後的圖片ID清單</param>
        /// <returns>是否排序成功</returns>
        Task<bool> ReorderPropertyImagesAsync(int propertyId, List<long> imageIds);

        /// <summary>
        /// 產生房源圖片 URL（封裝 ImageQueryService）
        /// </summary>
        /// <param name="storedFileName">儲存檔名</param>
        /// <param name="size">尺寸</param>
        /// <returns>完整 URL</returns>
        string GeneratePropertyImageUrl(string storedFileName, ImageSize size = ImageSize.Original);
    }
}