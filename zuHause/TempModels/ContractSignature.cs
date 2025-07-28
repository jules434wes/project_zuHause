using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 電子簽名儲存表
/// </summary>
[Table("contractSignatures")]
public partial class ContractSignature
{
    /// <summary>
    /// 電子簽名ID
    /// </summary>
    [Key]
    [Column("idcontractSignatureId")]
    public int IdcontractSignatureId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    [Column("contractId")]
    public int ContractId { get; set; }

    /// <summary>
    /// 簽約人ID
    /// </summary>
    [Column("signerId")]
    public int SignerId { get; set; }

    /// <summary>
    /// 簽署人身份
    /// </summary>
    [Column("signerRole")]
    [StringLength(20)]
    [Unicode(false)]
    public string SignerRole { get; set; } = null!;

    /// <summary>
    /// 簽名方式
    /// </summary>
    [Column("signMethod")]
    [StringLength(20)]
    public string? SignMethod { get; set; }

    /// <summary>
    /// 簽名檔URL
    /// </summary>
    [Column("signatureFileUrl")]
    [StringLength(255)]
    public string? SignatureFileUrl { get; set; }

    /// <summary>
    /// 簽署驗證資訊
    /// </summary>
    [Column("signVerifyInfo")]
    [StringLength(255)]
    public string? SignVerifyInfo { get; set; }

    /// <summary>
    /// 時間戳
    /// </summary>
    [Column("signedAt")]
    [Precision(0)]
    public DateTime? SignedAt { get; set; }

    [Column("uploadId")]
    public int? UploadId { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("ContractSignatures")]
    public virtual Contract Contract { get; set; } = null!;

    [ForeignKey("SignerId")]
    [InverseProperty("ContractSignatures")]
    public virtual Member Signer { get; set; } = null!;
}
