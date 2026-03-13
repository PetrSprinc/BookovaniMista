using Microsoft.EntityFrameworkCore;

namespace BookovaniMista.Data
{
    public class BookovaniMistaDbContext : DbContext
    {
        public DbSet<Models.Sekce> Sekce { get; set; }
        public BookovaniMistaDbContext(DbContextOptions<BookovaniMistaDbContext> options)
            : base(options)
        {
            
        }
    }
}
