using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 通訊相關驗證表
/// </summary>
public partial class MemberVerification
{
    /// <summary>
    /// 驗證ID
    /// </summary>
    public int VerificationId { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 驗證類型代碼
    /// </summary>
    public string VerificationTypeCode { get; set; } = null!;

    /// <summary>
    /// 驗證碼
    /// </summary>
    public string VerificationCode { get; set; } = null!;

    /// <summary>
    /// 是否驗證成功
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// 發送時間
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 驗證完成時間
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    public virtual Member Member { get; set; } = null!;
}
