using System.ComponentModel.DataAnnotations;

namespace AutenticacionSeguraAA01_JC.ViewModels;

public class CambiarEstadoViewModel
{
    [Required]
    public string IdUsuario { get; set; } = string.Empty;

    public bool Habilitar { get; set; }
}