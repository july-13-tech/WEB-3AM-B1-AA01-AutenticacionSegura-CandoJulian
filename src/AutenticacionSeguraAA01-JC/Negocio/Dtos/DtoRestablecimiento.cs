namespace AutenticacionSeguraAA01_JC.Negocio.Dtos;

public class DtoRestablecimiento
{
    public string IdUsuario { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}