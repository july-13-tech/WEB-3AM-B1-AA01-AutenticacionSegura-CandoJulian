using AutenticacionSeguraAA01_JC.Negocio.Dtos;

namespace AutenticacionSeguraAA01_JC.Negocio.Interfaces;

public interface IServicioAdministracionUsuarios
{
    Task<List<DtoUsuarioListado>> ListarUsuariosAsync();
    Task<DtoResultadoOperacion> CambiarRolAsync(DtoCambioRol dtoCambioRol);
    Task<DtoResultadoOperacion> CambiarEstadoAsync(DtoCambioEstado dtoCambioEstado);
}