using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zuHause.Models
{
    [Table("furnitureCategories")]
    public class FurnitureCategory
    {
        [Key]
        [Column("furnitureCategoriesId")]
        public string FurnitureCategoriesId { get; set; }

        public string? ParentId { get; set; }
        public string Name { get; set; }
        public byte Depth { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
