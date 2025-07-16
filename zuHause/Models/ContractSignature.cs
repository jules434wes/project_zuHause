using System;
using System.Collections.Generic;

namespace zuHause.Models;

/// <summary>
/// 電子簽名儲存表
/// </summary>
public partial class ContractSignature
{
    /// <summary>
    /// 電子簽名ID
    /// </summary>
    public int IdcontractSignatureId { get; set; }

    /// <summary>
    /// 合約ID
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// 簽約人ID
    /// </summary>
    public int SignerId { get; set; }

    /// <summary>
    /// 簽署人身份
    /// </summary>
    public string SignerRole { get; set; } = null!;

    /// <summary>
    /// 簽名方式
    /// </summary>
    public string? SignMethod { get; set; }

    /// <summary>
    /// 簽名檔URL
    /// </summary>
    public string? SignatureFileUrl { get; set; }

    /// <summary>
    /// 簽署驗證資訊
    /// </summary>
    public string? SignVerifyInfo { get; set; }

    /// <summary>
    /// 時間戳
    /// </summary>
    public DateTime? SignedAt { get; set; }

    public int? UploadId { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Member Signer { get; set; } = null!;
}
