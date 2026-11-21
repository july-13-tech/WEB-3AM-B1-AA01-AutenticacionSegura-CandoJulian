using AutenticacionSeguraAA01_JC.Comunes.Constantes;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSeguraAA01_JC.Negocio.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    
    private readonly UserManager<UsuarioAplicacion> gestorUsuarios;
    private readonly SignInManager<UsuarioAplicacion> gestorInicioSesion;

    public ServicioAutenticacion(
        UserManager<UsuarioAplicacion> gestorUsuarios,
        SignInManager<UsuarioAplicacion> gestorInicioSesion)
    {
        this.gestorUsuarios = gestorUsuarios;
        this.gestorInicioSesion = gestorInicioSesion;
    }

    public async Task<DtoResultadoOperacion> RegistrarAsync(DtoRegistro dtoRegistro)
    {
        var correoNormalizado = dtoRegistro.Correo.Trim();

        var usuarioExistente = await gestorUsuarios.FindByEmailAsync(correoNormalizado);

        if (usuarioExistente is not null)
        {
            return DtoResultadoOperacion.Fallido(new[] { ConstantesMensajes.MENSAJE_CORREO_REGISTRADO });
        }

        var usuarioAplicacion = new UsuarioAplicacion
        {
            UserName = correoNormalizado,
            Email = correoNormalizado,
            NombreCompleto = dtoRegistro.NombreCompleto.Trim(),
            Habilitado = true
        };

        var resultadoCreacion = await gestorUsuarios.CreateAsync(usuarioAplicacion, dtoRegistro.Contrasena);

        if (!resultadoCreacion.Succeeded)
        {
            return DtoResultadoOperacion.Fallido(resultadoCreacion.Errors.Select(identityError => identityError.Description));
        }

        var resultadoRol = await gestorUsuarios.AddToRoleAsync(usuarioAplicacion, ConstantesNombresRoles.CLIENTE);

        if (!resultadoRol.Succeeded)
        {
            await gestorUsuarios.DeleteAsync(usuarioAplicacion);
            return DtoResultadoOperacion.Fallido(resultadoRol.Errors.Select(identityError => identityError.Description));
        }

        return DtoResultadoOperacion.Correcto();
    }

    public async Task<DtoResultadoIngreso> IniciarSesionAsync(DtoIngreso dtoIngreso)
    {
        var usuarioAplicacion = await gestorUsuarios.FindByEmailAsync(dtoIngreso.Correo.Trim());

        if (usuarioAplicacion is null)
        {
            return DtoResultadoIngreso.Fallido(ConstantesMensajes.MENSAJE_CREDENCIALES_INVALIDAS);
        }else if (!usuarioAplicacion.Habilitado)
        {
            return DtoResultadoIngreso.Fallido(ConstantesMensajes.MENSAJE_CUENTA_DESHABILITADA);
        }

        var signInResult = await gestorInicioSesion.PasswordSignInAsync(
            usuarioAplicacion,
            dtoIngreso.Contrasena,
            dtoIngreso.Recordarme,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            return DtoResultadoIngreso.Correcto();
        }

        if (signInResult.IsLockedOut)
        {
            return DtoResultadoIngreso.Bloqueado(ConstantesMensajes.MENSAJE_CUENTA_BLOQUEADA);
        }

        return DtoResultadoIngreso.Fallido(ConstantesMensajes.MENSAJE_CREDENCIALES_INVALIDAS);
    }

    public async Task CerrarSesionAsync()
    {
        await gestorInicioSesion.SignOutAsync();
    }
}