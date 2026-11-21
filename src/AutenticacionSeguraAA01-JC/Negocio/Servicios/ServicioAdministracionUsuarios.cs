using AutenticacionSeguraAA01_JC.Comunes.Constantes;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutenticacionSeguraAA01_JC.Negocio.Servicios;

public class ServicioAdministracionUsuarios : IServicioAdministracionUsuarios
{
    private const string MensajeUsuarioNoEncontrado = "El usuario no existe.";
    private const string MensajeRolInvalido = "El rol seleccionado no es válido.";
    private const string MensajeUltimoAdministrador = "No se puede realizar la operación porque es el último administrador activo.";
    private const string MensajeOperacionFallida = "No se pudo completar la operación.";

    private readonly UserManager<UsuarioAplicacion> gestorUsuarios;

    public ServicioAdministracionUsuarios(UserManager<UsuarioAplicacion> gestorUsuarios)
    {
        this.gestorUsuarios = gestorUsuarios;
    }

    public async Task<List<DtoUsuarioListado>> ListarUsuariosAsync()
    {
        var usuariosAdministradores = await gestorUsuarios.GetUsersInRoleAsync(ConstantesNombresRoles.ADMINISTRADOR);
        var usuariosClientes = await gestorUsuarios.GetUsersInRoleAsync(ConstantesNombresRoles.CLIENTE);

        var idsAdministradores = usuariosAdministradores.Select(usuarioAplicacion => usuarioAplicacion.Id).ToHashSet();
        var idsClientes = usuariosClientes.Select(usuarioAplicacion => usuarioAplicacion.Id).ToHashSet();

        var usuariosAplicacion = await gestorUsuarios.Users
            .AsNoTracking()
            .OrderBy(usuarioAplicacion => usuarioAplicacion.NombreCompleto)
            .ToListAsync();

        return usuariosAplicacion
            .Select(usuarioAplicacion => new DtoUsuarioListado
            {
                IdUsuario = usuarioAplicacion.Id,
                NombreCompleto = usuarioAplicacion.NombreCompleto,
                Correo = usuarioAplicacion.Email ?? string.Empty,
                Rol = idsAdministradores.Contains(usuarioAplicacion.Id)
                    ? ConstantesNombresRoles.ADMINISTRADOR
                    : idsClientes.Contains(usuarioAplicacion.Id)
                        ? ConstantesNombresRoles.CLIENTE
                        : "Sin rol",
                Habilitado = usuarioAplicacion.Habilitado,
                FechaRegistro = usuarioAplicacion.FechaRegistro
            })
            .ToList();
    }

    public async Task<DtoResultadoOperacion> CambiarRolAsync(DtoCambioRol dtoCambioRol)
    {
        if (!ConstantesNombresRoles.TODOS.Contains(dtoCambioRol.NuevoRol))
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeRolInvalido });
        }

        var usuarioAplicacion = await BuscarUsuarioAsync(dtoCambioRol.IdUsuario);

        if (usuarioAplicacion is null)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeUsuarioNoEncontrado });
        }

        var rolesActuales = await gestorUsuarios.GetRolesAsync(usuarioAplicacion);
        var yaTieneNuevoRol = rolesActuales.Contains(dtoCambioRol.NuevoRol);

        if (yaTieneNuevoRol && rolesActuales.Count == 1)
        {
            return DtoResultadoOperacion.Correcto();
        }

        var esAdministrador = rolesActuales.Contains(ConstantesNombresRoles.ADMINISTRADOR);
        var perderaPrivilegios = esAdministrador && dtoCambioRol.NuevoRol != ConstantesNombresRoles.ADMINISTRADOR;

        if (perderaPrivilegios && usuarioAplicacion.Habilitado && await ContarAdministradoresActivosAsync() <= 1)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeUltimoAdministrador });
        }

        if (!yaTieneNuevoRol)
        {
            var resultadoAgregado = await gestorUsuarios.AddToRoleAsync(usuarioAplicacion, dtoCambioRol.NuevoRol);

            if (!resultadoAgregado.Succeeded)
            {
                return DtoResultadoOperacion.Fallido(new[] { MensajeOperacionFallida });
            }
        }

        var rolesPorQuitar = rolesActuales.Where(nombreRol => nombreRol != dtoCambioRol.NuevoRol).ToList();

        if (rolesPorQuitar.Count > 0)
        {
            var resultadoQuitado = await gestorUsuarios.RemoveFromRolesAsync(usuarioAplicacion, rolesPorQuitar);

            if (!resultadoQuitado.Succeeded)
            {
                if (!yaTieneNuevoRol)
                {
                    await gestorUsuarios.RemoveFromRoleAsync(usuarioAplicacion, dtoCambioRol.NuevoRol);
                }

                return DtoResultadoOperacion.Fallido(new[] { MensajeOperacionFallida });
            }
        }

        return DtoResultadoOperacion.Correcto();
    }

    public async Task<DtoResultadoOperacion> CambiarEstadoAsync(DtoCambioEstado dtoCambioEstado)
    {
        var usuarioAplicacion = await BuscarUsuarioAsync(dtoCambioEstado.IdUsuario);

        if (usuarioAplicacion is null)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeUsuarioNoEncontrado });
        }

        if (usuarioAplicacion.Habilitado == dtoCambioEstado.Habilitar)
        {
            return DtoResultadoOperacion.Correcto();
        }

        if (!dtoCambioEstado.Habilitar)
        {
            var esAdministrador = await gestorUsuarios.IsInRoleAsync(usuarioAplicacion, ConstantesNombresRoles.ADMINISTRADOR);

            if (esAdministrador && await ContarAdministradoresActivosAsync() <= 1)
            {
                return DtoResultadoOperacion.Fallido(new[] { MensajeUltimoAdministrador });
            }
        }

        usuarioAplicacion.Habilitado = dtoCambioEstado.Habilitar;
        var identityResult = await gestorUsuarios.UpdateAsync(usuarioAplicacion);

        if (!identityResult.Succeeded)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeOperacionFallida });
        }

        if (!dtoCambioEstado.Habilitar)
        {
            await gestorUsuarios.UpdateSecurityStampAsync(usuarioAplicacion);
        }

        return DtoResultadoOperacion.Correcto();
    }

    private async Task<UsuarioAplicacion?> BuscarUsuarioAsync(string idUsuario)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
        {
            return null;
        }

        return await gestorUsuarios.FindByIdAsync(idUsuario);
    }

    private async Task<int> ContarAdministradoresActivosAsync()
    {
        var usuariosAdministradores = await gestorUsuarios.GetUsersInRoleAsync(ConstantesNombresRoles.ADMINISTRADOR);
        return usuariosAdministradores.Count(usuarioAplicacion => usuarioAplicacion.Habilitado);
    }
}