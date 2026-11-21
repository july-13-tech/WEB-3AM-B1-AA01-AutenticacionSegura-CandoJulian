using AutenticacionSeguraAA01_JC.Datos.Entidades;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSeguraAA01_JC.Negocio.Servicios;

public class ServicioPerfil : IServicioPerfil
{
    private const string MensajeUsuarioNoDisponible = "No se pudo completar la operación. Inicia sesión nuevamente.";
    private const string MensajeContrasenaIgual = "La nueva contraseña debe ser distinta de la actual.";

    private readonly UserManager<UsuarioAplicacion> gestorUsuarios;
    private readonly SignInManager<UsuarioAplicacion> gestorInicioSesion;

    public ServicioPerfil(
        UserManager<UsuarioAplicacion> gestorUsuarios,
        SignInManager<UsuarioAplicacion> gestorInicioSesion)
    {
        this.gestorUsuarios = gestorUsuarios;
        this.gestorInicioSesion = gestorInicioSesion;
    }

    public async Task<DtoPerfilUsuario?> ObtenerPerfilAsync(string idUsuario)
    {
        var usuarioAplicacion = await ObtenerUsuarioHabilitadoAsync(idUsuario);

        if (usuarioAplicacion is null)
        {
            return null;
        }

        var roles = await gestorUsuarios.GetRolesAsync(usuarioAplicacion);

        return new DtoPerfilUsuario
        {
            NombreCompleto = usuarioAplicacion.NombreCompleto,
            Correo = usuarioAplicacion.Email ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? "Sin rol",
            Habilitado = usuarioAplicacion.Habilitado,
            FechaRegistro = usuarioAplicacion.FechaRegistro
        };
    }

    public async Task<DtoResultadoOperacion> CambiarContrasenaAsync(DtoCambioContrasena dtoCambioContrasena)
    {
        var usuarioAplicacion = await ObtenerUsuarioHabilitadoAsync(dtoCambioContrasena.IdUsuario);

        if (usuarioAplicacion is null)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeUsuarioNoDisponible });
        }

        if (dtoCambioContrasena.ContrasenaActual == dtoCambioContrasena.NuevaContrasena)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeContrasenaIgual });
        }

        var identityResult = await gestorUsuarios.ChangePasswordAsync(
            usuarioAplicacion,
            dtoCambioContrasena.ContrasenaActual,
            dtoCambioContrasena.NuevaContrasena);

        if (!identityResult.Succeeded)
        {
            return DtoResultadoOperacion.Fallido(identityResult.Errors.Select(identityError => identityError.Description));
        }

        await gestorInicioSesion.RefreshSignInAsync(usuarioAplicacion);

        return DtoResultadoOperacion.Correcto();
    }

    private async Task<UsuarioAplicacion?> ObtenerUsuarioHabilitadoAsync(string idUsuario)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
        {
            return null;
        }

        var usuarioAplicacion = await gestorUsuarios.FindByIdAsync(idUsuario);

        if (usuarioAplicacion is null || !usuarioAplicacion.Habilitado)
        {
            return null;
        }

        return usuarioAplicacion;
    }
}