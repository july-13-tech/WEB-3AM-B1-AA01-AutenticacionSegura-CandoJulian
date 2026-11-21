using AutenticacionSeguraAA01_JC.Negocio.Dtos;

namespace AutenticacionSeguraAA01_JC.Negocio.Interfaces;

public interface IServicioPerfil
{
    Task<DtoPerfilUsuario?> ObtenerPerfilAsync(string idUsuario);
    Task<DtoResultadoOperacion> CambiarContrasenaAsync(DtoCambioContrasena dtoCambioContrasena);
}