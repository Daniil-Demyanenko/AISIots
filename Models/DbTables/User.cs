using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class User
{
    [Key] public string Login { get; set; } = string.Empty;
    public byte[] Password { get; set; } = [];

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}