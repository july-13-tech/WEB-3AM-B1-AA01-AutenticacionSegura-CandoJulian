namespace AutenticacionSeguraAA01_JC.Negocio.Dtos;

public class DtoSolicitudRecuperacion
{
    public string Correo { get; set; } = string.Empty;
    public string UrlBaseRestablecimiento { get; set; } = string.Empty;
}