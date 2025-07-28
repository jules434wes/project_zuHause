using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 合約附表(動態欄位)
/// </summary>
[Table("contractCustomFields")]
public partial class ContractCustomField
{
    /// <summary>
    /// 動態欄位ID
    /// </summary>
    [Key]
    [Column("contractCustomFieldId")]
    public int ContractCustomFieldId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    [Column("contractId")]
    public int ContractId { get; set; }

    /// <summary>
    /// 動態欄位名稱
    /// </summary>
    [Column("fieldKey")]
    [StringLength(30)]
    public string FieldKey { get; set; } = null!;

    /// <summary>
    /// 動態欄位值
    /// </summary>
    [Column("fieldValue")]
    [StringLength(100)]
    public string? FieldValue { get; set; }

    /// <summary>
    /// 動態欄位型態
    /// </summary>
    [Column("fieldType")]
    [StringLength(20)]
    [Unicode(false)]
    public string? FieldType { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    [Column("displayOrder")]
    public int DisplayOrder { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("ContractCustomFields")]
    public virtual Contract Contract { get; set; } = null!;
}
