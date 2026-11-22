using System.ComponentModel.DataAnnotations;

namespace BSUIR_Savchenko.Domain.Entities;

public class Book
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }

    public string? Image { get; set; }

    public string? ImageMimeType { get; set; }
}
