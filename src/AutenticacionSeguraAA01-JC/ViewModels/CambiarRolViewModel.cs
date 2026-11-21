using System.ComponentModel.DataAnnotations;

namespace AutenticacionSeguraAA01_JC.ViewModels;

public class CambiarRolViewModel
{
    [Required]
    public string IdUsuario { get; set; } = string.Empty;

    [Required]
    public string NuevoRol { get; set; } = string.Empty;
}