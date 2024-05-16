using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class User
{
    [Key] public string Login { get; set; }
    public byte[] Password { get; set; }
}