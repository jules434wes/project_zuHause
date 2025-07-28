using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

/// <summary>
/// 房源投訴表
/// </summary>
[Table("propertyComplaints")]
[Index("ComplainantId", Name = "IX_propertyComplaints_complainantId")]
[Index("CreatedAt", Name = "IX_propertyComplaints_createdAt")]
[Index("HandledBy", Name = "IX_propertyComplaints_handledBy")]
[Index("LandlordId", Name = "IX_propertyComplaints_landlordId")]
[Index("PropertyId", Name = "IX_propertyComplaints_propertyId")]
[Index("ResolvedAt", Name = "IX_propertyComplaints_resolvedAt")]
[Index("StatusCode", Name = "IX_propertyComplaints_statusCode")]
public partial class PropertyComplaint
{
    /// <summary>
    /// 投訴ID (自動遞增，從301開始)
    /// </summary>
    [Key]
    [Column("complaintId")]
    public int ComplaintId { get; set; }

    /// <summary>
    /// 投訴人ID
    /// </summary>
    [Column("complainantId")]
    public int ComplainantId { get; set; }

    /// <summary>
    /// 房源ID
    /// </summary>
    [Column("propertyId")]
    public int PropertyId { get; set; }

    /// <summary>
    /// 被投訴房東ID
    /// </summary>
    [Column("landlordId")]
    public int LandlordId { get; set; }

    /// <summary>
    /// 投訴內容
    /// </summary>
    [Column("complaintContent")]
    public string ComplaintContent { get; set; } = null!;

    /// <summary>
    /// 處理狀態代碼
    /// </summary>
    [Column("statusCode")]
    [StringLength(20)]
    [Unicode(false)]
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column("createdAt")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column("updatedAt")]
    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 結案時間
    /// </summary>
    [Column("resolvedAt")]
    [Precision(0)]
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// 內部註記
    /// </summary>
    [Column("internalNote")]
    public string? InternalNote { get; set; }

    /// <summary>
    /// 處理人員ID
    /// </summary>
    [Column("handledBy")]
    public int? HandledBy { get; set; }

    [ForeignKey("ComplainantId")]
    [InverseProperty("PropertyComplaintComplainants")]
    public virtual Member Complainant { get; set; } = null!;

    [ForeignKey("LandlordId")]
    [InverseProperty("PropertyComplaintLandlords")]
    public virtual Member Landlord { get; set; } = null!;

    [ForeignKey("PropertyId")]
    [InverseProperty("PropertyComplaints")]
    public virtual Property Property { get; set; } = null!;
}
