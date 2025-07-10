using Microsoft.EntityFrameworkCore;
using zuHause.Models;


namespace zuHause.Data
{
    public class ZuhauseDbContext : DbContext
    {
        public ZuhauseDbContext(DbContextOptions<ZuhauseDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; }
    }
}
