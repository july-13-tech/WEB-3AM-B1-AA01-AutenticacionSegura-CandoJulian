using AutenticacionSeguraAA01_JC.Comunes.Constantes;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AutenticacionSeguraAA01_JC.Datos.Seed;

public class InicializadorIdentidad
{
    private readonly UserManager<UsuarioAplicacion> gestorUsuarios;
    private readonly RoleManager<IdentityRole> gestorRoles;
    private readonly OpcionesAdministrador opcionesAdministrador;
    private readonly ILogger<InicializadorIdentidad> registrador;

    public InicializadorIdentidad(
        UserManager<UsuarioAplicacion> gestorUsuarios,
        RoleManager<IdentityRole> gestorRoles,
        IOptions<OpcionesAdministrador> opcionesAdministrador,
        ILogger<InicializadorIdentidad> registrador)
    {
        this.gestorUsuarios = gestorUsuarios;
        this.gestorRoles = gestorRoles;
        this.opcionesAdministrador = opcionesAdministrador.Value;
        this.registrador = registrador;
    }

    public async Task InicializarAsync()
    {
        await CrearRolesAsync();
        await CrearAdministradorAsync();
    }

    private async Task CrearRolesAsync()
    {
        foreach (var nombreRol in ConstantesNombresRoles.TODOS)
        {
            if (await gestorRoles.RoleExistsAsync(nombreRol))
            {
                continue;
            }

            var identityResult = await gestorRoles.CreateAsync(new IdentityRole(nombreRol));

            if (!identityResult.Succeeded)
            {
                var detalleErrores = string.Join("; ", identityResult.Errors.Select(identityError => identityError.Description));
                throw new InvalidOperationException($"No se pudo crear el rol {nombreRol}: {detalleErrores}");
            }
        }
    }

    private async Task CrearAdministradorAsync()
    {
        if (string.IsNullOrWhiteSpace(opcionesAdministrador.Correo)
            || string.IsNullOrWhiteSpace(opcionesAdministrador.Contrasena))
        {
            registrador.LogWarning(ConstantesMensajes.MENSAJE_DATOS_FALTANTES_CREACION_ADMINISTRADOR);
            return;
        }

        var usuarioAplicacion = await gestorUsuarios.FindByEmailAsync(opcionesAdministrador.Correo);

        if (usuarioAplicacion is null)
        {
            usuarioAplicacion = new UsuarioAplicacion
            {
                UserName = opcionesAdministrador.Correo,
                Email = opcionesAdministrador.Correo,
                EmailConfirmed = true,
                NombreCompleto = string.IsNullOrWhiteSpace(opcionesAdministrador.Nombre)
                    ? ConstantesMensajes.NOMBRE_GENERICO_ROL_ADMINISTRADOR
                    : opcionesAdministrador.Nombre,
                Habilitado = true
            };

            var identityResult = await gestorUsuarios.CreateAsync(usuarioAplicacion, opcionesAdministrador.Contrasena);

            if (!identityResult.Succeeded)
            {
                var detalleErrores = string.Join("; ", identityResult.Errors.Select(identityError => identityError.Description));
                throw new InvalidOperationException($"No se pudo crear el administrador inicial: {detalleErrores}");
            }
        }

        if (!await gestorUsuarios.IsInRoleAsync(usuarioAplicacion, ConstantesNombresRoles.ADMINISTRADOR))
        {
            await gestorUsuarios.AddToRoleAsync(usuarioAplicacion, ConstantesNombresRoles.ADMINISTRADOR);
        }
    }
}