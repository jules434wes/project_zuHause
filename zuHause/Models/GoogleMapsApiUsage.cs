using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models
{
    /// <summary>
    /// Google Maps API 使用量追蹤模型
    /// </summary>
    [Table("GoogleMapsApiUsage")]
    public class GoogleMapsApiUsage
    {
        /// <summary>
        /// Google Maps API 使用量ID
        /// </summary>
        [Key]
        [Column("googleMapsApiId")]
        public int GoogleMapsApiId { get; set; }

        /// <summary>
        /// API 類型
        /// </summary>
        [Column("apiType")]
        [MaxLength(50)]
        public string ApiType { get; set; } = string.Empty;

        /// <summary>
        /// 請求日期
        /// </summary>
        [Column("requestDate")]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// 當日請求次數
        /// </summary>
        [Column("requestCount")]
        public int RequestCount { get; set; }

        /// <summary>
        /// 預估成本
        /// </summary>
        [Column("estimatedCost", TypeName = "decimal(10,4)")]
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// 是否達到限制
        /// </summary>
        [Column("isLimitReached")]
        public bool IsLimitReached { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}