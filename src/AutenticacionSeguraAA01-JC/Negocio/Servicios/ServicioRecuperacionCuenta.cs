using System.Net;
using System.Text;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace AutenticacionSeguraAA01_JC.Negocio.Servicios;

public class ServicioRecuperacionCuenta : IServicioRecuperacionCuenta
{
    private const string MensajeEnlaceInvalido = "El enlace no es válido o ha vencido. Solicita uno nuevo.";

    private readonly UserManager<UsuarioAplicacion> gestorUsuarios;
    private readonly IEmailService emailService;
    private readonly DataProtectionTokenProviderOptions opcionesToken;
    private readonly ILogger<ServicioRecuperacionCuenta> registrador;

    public ServicioRecuperacionCuenta(
        UserManager<UsuarioAplicacion> gestorUsuarios,
        IEmailService emailService,
        IOptions<DataProtectionTokenProviderOptions> opcionesToken,
        ILogger<ServicioRecuperacionCuenta> registrador)
    {
        this.gestorUsuarios = gestorUsuarios;
        this.emailService = emailService;
        this.opcionesToken = opcionesToken.Value;
        this.registrador = registrador;
    }

    public async Task SolicitarRecuperacionAsync(DtoSolicitudRecuperacion dtoSolicitudRecuperacion)
    {
        var usuarioAplicacion = await gestorUsuarios.FindByEmailAsync(dtoSolicitudRecuperacion.Correo.Trim());

        if (usuarioAplicacion is null || !usuarioAplicacion.Habilitado)
        {
            return;
        }

        var tokenOriginal = await gestorUsuarios.GeneratePasswordResetTokenAsync(usuarioAplicacion);
        var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenOriginal));

        var enlace = QueryHelpers.AddQueryString(
            dtoSolicitudRecuperacion.UrlBaseRestablecimiento,
            new Dictionary<string, string?>
            {
                ["idUsuario"] = usuarioAplicacion.Id,
                ["token"] = tokenCodificado
            });

        var minutosVigencia = (int)opcionesToken.TokenLifespan.TotalMinutes;
        var nombreSeguro = WebUtility.HtmlEncode(usuarioAplicacion.NombreCompleto);
        var enlaceSeguro = WebUtility.HtmlEncode(enlace);

        var dtoMensajeCorreo = new DtoMensajeCorreo
        {
            Destinatario = usuarioAplicacion.Email!,
            Asunto = "Recuperación de contraseña - Mundo Motriz JC",
            CuerpoHtml =
                $"<p>Hola {nombreSeguro},</p>" +
                "<p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>" +
                $"<p><a href=\"{enlaceSeguro}\">Restablecer mi contraseña</a></p>" +
                $"<p>El enlace es válido por {minutosVigencia} minutos y solo puede usarse una vez.</p>" +
                "<p>Si no hiciste esta solicitud, ignora este mensaje: tu contraseña no cambiará.</p>",
            CuerpoTexto =
                $"Hola {usuarioAplicacion.NombreCompleto},\n\n" +
                "Recibimos una solicitud para restablecer la contraseña de tu cuenta.\n" +
                $"Abre este enlace para continuar: {enlace}\n\n" +
                $"El enlace es válido por {minutosVigencia} minutos y solo puede usarse una vez.\n" +
                "Si no hiciste esta solicitud, ignora este mensaje: tu contraseña no cambiará."
        };

        try
        {
            await emailService.EnviarAsync(dtoMensajeCorreo);
        }
        catch (Exception excepcion)
        {
            registrador.LogError(excepcion, "No se pudo enviar el correo de recuperación al usuario {IdUsuario}.", usuarioAplicacion.Id);
        }
    }

    public async Task<bool> ValidarEnlaceAsync(string idUsuario, string token)
    {
        var usuarioAplicacion = await ObtenerUsuarioHabilitadoAsync(idUsuario);
        var tokenOriginal = DecodificarToken(token);

        if (usuarioAplicacion is null || tokenOriginal is null)
        {
            return false;
        }

        return await gestorUsuarios.VerifyUserTokenAsync(
            usuarioAplicacion,
            gestorUsuarios.Options.Tokens.PasswordResetTokenProvider,
            UserManager<UsuarioAplicacion>.ResetPasswordTokenPurpose,
            tokenOriginal);
    }

    public async Task<DtoResultadoOperacion> RestablecerContrasenaAsync(DtoRestablecimiento dtoRestablecimiento)
    {
        var usuarioAplicacion = await ObtenerUsuarioHabilitadoAsync(dtoRestablecimiento.IdUsuario);
        var tokenOriginal = DecodificarToken(dtoRestablecimiento.Token);

        if (usuarioAplicacion is null || tokenOriginal is null)
        {
            return DtoResultadoOperacion.Fallido(new[] { MensajeEnlaceInvalido });
        }

        var identityResult = await gestorUsuarios.ResetPasswordAsync(
            usuarioAplicacion,
            tokenOriginal,
            dtoRestablecimiento.NuevaContrasena);

        if (!identityResult.Succeeded)
        {
            return DtoResultadoOperacion.Fallido(identityResult.Errors.Select(identityError => identityError.Description));
        }

        await gestorUsuarios.ResetAccessFailedCountAsync(usuarioAplicacion);
        await gestorUsuarios.SetLockoutEndDateAsync(usuarioAplicacion, null);

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

    private static string? DecodificarToken(string tokenCodificado)
    {
        if (string.IsNullOrWhiteSpace(tokenCodificado))
        {
            return null;
        }

        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(tokenCodificado));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}