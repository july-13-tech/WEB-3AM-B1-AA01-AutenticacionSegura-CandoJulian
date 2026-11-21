using AutenticacionSeguraAA01_JC.Negocio.Dtos;

namespace AutenticacionSeguraAA01_JC.Negocio.Interfaces;

public interface IEmailService
{
    Task EnviarAsync(DtoMensajeCorreo dtoMensajeCorreo);
}