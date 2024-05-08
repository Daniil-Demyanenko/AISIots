using System.ComponentModel.DataAnnotations;

namespace AISIots.Models;

public class Plan
{
    [Key]
    public int Id { get; set; }
    public string Profile { get; set; }
    public string Code { get; set; }
    public string Level { get; set; }
}