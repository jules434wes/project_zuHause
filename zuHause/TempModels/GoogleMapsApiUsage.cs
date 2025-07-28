using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace zuHause.TempModels;

[Table("GoogleMapsApiUsage")]
public partial class GoogleMapsApiUsage
{
    [Key]
    [Column("googleMapsApiId")]
    public int GoogleMapsApiId { get; set; }

    [Column("apiType")]
    [StringLength(50)]
    public string ApiType { get; set; } = null!;

    [Column("requestDate")]
    public DateOnly RequestDate { get; set; }

    [Column("requestCount")]
    public int RequestCount { get; set; }

    [Column("estimatedCost", TypeName = "decimal(10, 4)")]
    public decimal EstimatedCost { get; set; }

    [Column("isLimitReached")]
    public bool IsLimitReached { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }
}
