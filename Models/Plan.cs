using System.ComponentModel.DataAnnotations;

namespace AISIots.Models;

public class Plan
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
}