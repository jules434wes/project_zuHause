using Microsoft.EntityFrameworkCore;
using zuHause.Models;


namespace zuHause.Data
{
    public class ZuhauseDbContext : DbContext
    {
        public ZuhauseDbContext(DbContextOptions<ZuhauseDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<FurnitureProduct> FurnitureProducts { get; set; }
        public DbSet<FurnitureCategory> FurnitureCategories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FurnitureProduct>()
                .HasKey(p => p.FurnitureProductId);


            modelBuilder.Entity<FurnitureCategory>()
                .HasKey(c => c.FurnitureCategoriesId);

            // 如果使用 Fluent API 對應欄位，也可寫在這邊
        }
    }
}
