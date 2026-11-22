using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BSUIR_Savchenko.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string NormalizedName { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
