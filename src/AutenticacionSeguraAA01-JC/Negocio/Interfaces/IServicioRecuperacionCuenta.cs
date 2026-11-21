using AutenticacionSeguraAA01_JC.Negocio.Dtos;

namespace AutenticacionSeguraAA01_JC.Negocio.Interfaces;

public interface IServicioRecuperacionCuenta
{
    Task SolicitarRecuperacionAsync(DtoSolicitudRecuperacion dtoSolicitudRecuperacion);
    Task<bool> ValidarEnlaceAsync(string idUsuario, string token);
    Task<DtoResultadoOperacion> RestablecerContrasenaAsync(DtoRestablecimiento dtoRestablecimiento);
}