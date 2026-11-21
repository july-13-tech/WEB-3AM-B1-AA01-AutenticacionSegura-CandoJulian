namespace AutenticacionSeguraAA01_JC.Infraestructura.Correo;

public class OpcionesSmtp
{
    public const string NombreSeccion = "Smtp";

    public string Servidor { get; set; } = string.Empty;
    public int Puerto { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string CorreoRemitente { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
}