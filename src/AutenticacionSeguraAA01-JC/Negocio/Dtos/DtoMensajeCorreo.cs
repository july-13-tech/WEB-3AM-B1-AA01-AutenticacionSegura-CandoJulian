namespace AutenticacionSeguraAA01_JC.Negocio.Dtos;

public class DtoMensajeCorreo
{
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string CuerpoHtml { get; set; } = string.Empty;
    public string CuerpoTexto { get; set; } = string.Empty;
}