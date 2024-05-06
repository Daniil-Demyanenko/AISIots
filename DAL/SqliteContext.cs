using AISIots.Models;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public sealed class SqliteContext : DbContext
{
    public DbSet<Plan>? Plans { get; set; }
    
    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}