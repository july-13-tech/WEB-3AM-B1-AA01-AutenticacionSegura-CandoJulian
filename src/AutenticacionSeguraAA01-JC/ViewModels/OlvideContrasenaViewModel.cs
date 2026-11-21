using System.ComponentModel.DataAnnotations;

namespace AutenticacionSeguraAA01_JC.ViewModels;

public class OlvideContrasenaViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;
}