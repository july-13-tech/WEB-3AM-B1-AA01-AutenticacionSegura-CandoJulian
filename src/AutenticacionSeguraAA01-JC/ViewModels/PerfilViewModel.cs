namespace AutenticacionSeguraAA01_JC.ViewModels;

public class PerfilViewModel
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public DateTime FechaRegistro { get; set; }
}