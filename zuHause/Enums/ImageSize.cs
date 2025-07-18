namespace zuHause.Enums
{
    /// <summary>
    /// 圖片尺寸枚舉，用於生成不同尺寸的圖片 URL
    /// </summary>
    public enum ImageSize
    {
        /// <summary>
        /// 原始尺寸
        /// </summary>
        Original,
        
        /// <summary>
        /// 大尺寸 (1920x1080)
        /// </summary>
        Large,
        
        /// <summary>
        /// 中等尺寸 (800x600)
        /// </summary>
        Medium,
        
        /// <summary>
        /// 縮圖尺寸 (300x200)
        /// </summary>
        Thumbnail
    }
}