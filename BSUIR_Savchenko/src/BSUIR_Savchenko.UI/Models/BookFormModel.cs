using System.ComponentModel.DataAnnotations;

namespace BSUIR_Savchenko.UI.Models;

public class BookFormModel
{
    [Required(ErrorMessage = "Введите название."), StringLength(120)]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание."), StringLength(600)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию.")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Цена должна быть больше нуля.")]
    [Display(Name = "Цена, руб.")]
    public decimal Price { get; set; }
}
