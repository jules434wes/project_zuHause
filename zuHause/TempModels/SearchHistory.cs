using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 搜索歷史表
/// </summary>
[Table("searchHistory")]
[Index("MemberId", "SearchedAt", Name = "IX_searchHistory_member_time")]
public partial class SearchHistory
{
    /// <summary>
    /// 歷史ID
    /// </summary>
    [Key]
    [Column("historyID")]
    public long HistoryId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    [Column("keyword")]
    [StringLength(255)]
    public string Keyword { get; set; } = null!;

    /// <summary>
    /// 結果筆數
    /// </summary>
    [Column("resultCount")]
    public int? ResultCount { get; set; }

    /// <summary>
    /// 裝置
    /// </summary>
    [Column("deviceType")]
    [StringLength(10)]
    [Unicode(false)]
    public string? DeviceType { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    [Column("ipAddress")]
    [StringLength(45)]
    [Unicode(false)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 搜尋時間
    /// </summary>
    [Column("searchedAt")]
    [Precision(0)]
    public DateTime SearchedAt { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("SearchHistories")]
    public virtual Member Member { get; set; } = null!;
}
