using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<Rpd> Rpds { get; init; }
    public DbSet<Plan> Plans { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<Role> Roles { get; init; }
    public DbSet<ActionLog> ActionLogs { get; init; }

    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();

        var dbConnection = Database.GetDbConnection();
        dbConnection.Open();
        using var command = dbConnection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000; PRAGMA synchronous=NORMAL;";
        command.ExecuteNonQuery();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Rpd>().HasQueryFilter(r => !r.IsDeleted);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "MainAdmin" },
            new Role { Id = 2, Name = "Admin" },
            new Role { Id = 3, Name = "User" }
        );

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