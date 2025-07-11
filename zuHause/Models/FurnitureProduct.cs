using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models
{
    [Table("furnitureProducts")]
    public class FurnitureProduct
    {
        [Key]
        [Column("furnitureProductId")]
        public string FurnitureProductId { get; set; }

        public string CategoryId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DailyRental { get; set; }
        public string ImageUrl { get; set; }
        public bool Status { get; set; }
        public DateTime? ListedAt { get; set; }//上架時間 
        public DateTime? DelistedAt { get; set; }//下架時間 
        public DateTime? CreatedAt { get; set; }//建立時間 
        public DateTime? UpdatedAt { get; set; }//更新時間 
        public DateTime? DeletedAt { get; set; }//刪除時間 
    }
}
