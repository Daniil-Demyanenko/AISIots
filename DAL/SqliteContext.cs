using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<Rpd>? RPD { get; set; }
    public DbSet<Plan>? Plans { get; set; }
    
    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RpdConfiguration());
        
        modelBuilder.Entity<PlanBlock>()
            .HasMany(p => p.BlockSections)
            .WithOne()
            .HasForeignKey(b => b.Id);

        modelBuilder.Entity<BlockSection>()
            .HasMany(s => s.ShortRpds)
            .WithOne()
            .HasForeignKey(s => s.Id);
    }
}