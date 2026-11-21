using AutenticacionSeguraAA01_JC.Negocio.Dtos;

namespace AutenticacionSeguraAA01_JC.Negocio.Interfaces
{
    public interface IServicioAutenticacion
    {
        Task<DtoResultadoOperacion> RegistrarAsync(DtoRegistro dtoRegistro);
        Task<DtoResultadoIngreso> IniciarSesionAsync(DtoIngreso dtoIngreso);
        Task CerrarSesionAsync();
    }
}
