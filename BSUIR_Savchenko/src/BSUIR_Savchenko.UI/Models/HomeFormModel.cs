using System.ComponentModel.DataAnnotations;

namespace BSUIR_Savchenko.UI.Models;

public class HomeFormModel
{
    public string SelectedItem { get; set; } = "item1";
    public bool CheckOne { get; set; }
    public bool CheckTwo { get; set; }
    public string RadioOption { get; set; } = "radio1";

    [Required(ErrorMessage = "Укажите логин.")]
    [EmailAddress(ErrorMessage = "Введите корректную электронную почту.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
