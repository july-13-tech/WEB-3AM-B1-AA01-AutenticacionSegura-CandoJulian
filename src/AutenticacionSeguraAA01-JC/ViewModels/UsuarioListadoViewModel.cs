namespace AutenticacionSeguraAA01_JC.ViewModels;

public class UsuarioListadoViewModel
{
    public string IdUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool EsUsuarioActual { get; set; }
}