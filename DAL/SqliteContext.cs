using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<Rpd> Rpds { get; init; }
    public DbSet<Plan> Plans { get; init; }
    public DbSet<User> Users { get; init; }

    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DisciplineBlock>()
            .HasMany(p => p.DisciplineSections)
            .WithOne()
            .HasForeignKey(b => b.DisciplineBlockId);

        modelBuilder.Entity<DisciplineSection>()
            .HasMany(s => s.ShortRpds)
            .WithOne() 
            .HasForeignKey(r => r.DisciplineSectionId);
    }
}