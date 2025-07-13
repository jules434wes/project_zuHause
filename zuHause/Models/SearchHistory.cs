using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 搜索歷史表
/// </summary>
public partial class SearchHistory
{
    /// <summary>
    /// 歷史ID
    /// </summary>
    public long HistoryId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string Keyword { get; set; } = null!;

    /// <summary>
    /// 結果筆數
    /// </summary>
    public int? ResultCount { get; set; }

    /// <summary>
    /// 裝置
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 搜尋時間
    /// </summary>
    public DateTime SearchedAt { get; set; }

    public virtual Member Member { get; set; } = null!;
}
