namespace AutenticacionSeguraAA01_JC.Negocio.Dtos;

public class DtoCambioContrasena
{
    public string IdUsuario { get; set; } = string.Empty;
    public string ContrasenaActual { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}