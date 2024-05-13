using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<Rpd>? Rpds { get; set; }
    public DbSet<Plan>? Plans { get; set; }

    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanBlock>()
            .HasMany(p => p.BlockSections)
            .WithOne()
            .HasForeignKey(b => b.PlanBlockId);

        modelBuilder.Entity<BlockSection>()
            .HasMany(s => s.ShortRpds)
            .WithOne() 
            .HasForeignKey(r => r.BlockSectionId); 
    }
}