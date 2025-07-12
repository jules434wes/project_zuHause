using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 合約附表(動態欄位)
/// </summary>
public partial class ContractCustomField
{
    /// <summary>
    /// 動態欄位ID
    /// </summary>
    public int ContractCustomFieldId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// 動態欄位名稱
    /// </summary>
    public string FieldKey { get; set; } = null!;

    /// <summary>
    /// 動態欄位值
    /// </summary>
    public string? FieldValue { get; set; }

    /// <summary>
    /// 動態欄位型態
    /// </summary>
    public string? FieldType { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
