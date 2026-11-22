using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BSUIR_Savchenko.API.Models;

public class BookForm
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }

    public IFormFile? ImageFile { get; set; }
}
