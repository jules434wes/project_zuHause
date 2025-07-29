using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 通訊相關驗證表
/// </summary>
[Table("memberVerifications")]
public partial class MemberVerification
{
    /// <summary>
    /// 驗證ID
    /// </summary>
    [Key]
    [Column("verificationID")]
    public int VerificationId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    [Column("memberID")]
    public int MemberId { get; set; }

    /// <summary>
    /// 驗證類型代碼
    /// </summary>
    [Column("verificationTypeCode")]
    [StringLength(20)]
    public string VerificationTypeCode { get; set; } = null!;

    /// <summary>
    /// 驗證碼
    /// </summary>
    [Column("verificationCode")]
    [StringLength(10)]
    public string VerificationCode { get; set; } = null!;

    /// <summary>
    /// 是否驗證成功
    /// </summary>
    [Column("isSuccessful")]
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    [Column("sentAt")]
    [Precision(0)]
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 驗證完成時間
    /// </summary>
    [Column("verifiedAt")]
    [Precision(0)]
    public DateTime? VerifiedAt { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MemberVerifications")]
    public virtual Member Member { get; set; } = null!;
}
