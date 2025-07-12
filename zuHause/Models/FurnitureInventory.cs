using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models
{
    [Table("furnitureInventory")]
    public class FurnitureInventory
    {
        [Key]
        [Column("furnitureInventoryId")]
        public string? FurnitureInventoryId { get; set; }

        public string? ProductId { get; set; }
        public int TotalQuantity { get; set; }
        public int RentedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int SafetyStock { get; set; }
        public DateTime UpdatedAt { get; set; }
        

    }
}
