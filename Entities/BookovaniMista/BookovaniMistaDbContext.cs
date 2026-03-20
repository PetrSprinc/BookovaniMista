using Entities.BookovaniMista.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.BookovaniMista
{
    public class BookovaniMistaDbContext : DbContext
    {
        public DbSet<Misto> Mista { get; set; } = null!;
        public DbSet<Rezervace> Rezervace { get; set; } = null!;
        public DbSet<Sekce> Sekce { get; set; } = null!;
        public DbSet<Zamestnanec> Zamestnanci { get; set; } = null!;

        public BookovaniMistaDbContext(DbContextOptions<BookovaniMistaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Sekce (1) -> Mista (N)
            modelBuilder.Entity<Sekce>()
                .HasMany(s => s.Mista)
                .WithOne(m => m.Sekce)
                .HasForeignKey(m => m.SekceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Misto (1) -> Rezervace (N)
            modelBuilder.Entity<Misto>()
                .HasMany(m => m.Rezervace)
                .WithOne(r => r.Misto)
                .HasForeignKey(r => r.MistoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Zamestnanec (1) -> Rezervace (N)
            modelBuilder.Entity<Zamestnanec>()
                .HasMany(z => z.Rezervace)
                .WithOne(r => r.Zamestnanec)
                .HasForeignKey(r => r.ZamestnanecId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
