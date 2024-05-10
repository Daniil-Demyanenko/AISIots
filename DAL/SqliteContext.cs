using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<RPD>? RPD { get; set; }
    
    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}