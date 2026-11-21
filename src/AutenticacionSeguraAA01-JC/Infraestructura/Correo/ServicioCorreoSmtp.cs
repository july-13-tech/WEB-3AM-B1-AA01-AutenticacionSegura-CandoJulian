using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutenticacionSeguraAA01_JC.Infraestructura.Correo;

public class ServicioCorreoSmtp : IEmailService
{
    private readonly OpcionesSmtp opcionesSmtp;
    private readonly ILogger<ServicioCorreoSmtp> registrador;

    public ServicioCorreoSmtp(
        IOptions<OpcionesSmtp> opcionesSmtp,
        ILogger<ServicioCorreoSmtp> registrador)
    {
        this.opcionesSmtp = opcionesSmtp.Value;
        this.registrador = registrador;
    }

    public async Task EnviarAsync(DtoMensajeCorreo dtoMensajeCorreo)
    {
        ValidarConfiguracion();

        var mensajeMime = new MimeMessage();
        mensajeMime.From.Add(new MailboxAddress(opcionesSmtp.NombreRemitente, opcionesSmtp.CorreoRemitente));
        mensajeMime.To.Add(MailboxAddress.Parse(dtoMensajeCorreo.Destinatario));
        mensajeMime.Subject = dtoMensajeCorreo.Asunto;

        var constructorCuerpo = new BodyBuilder
        {
            HtmlBody = dtoMensajeCorreo.CuerpoHtml,
            TextBody = dtoMensajeCorreo.CuerpoTexto
        };
        mensajeMime.Body = constructorCuerpo.ToMessageBody();

        using var clienteSmtp = new SmtpClient();
        clienteSmtp.Timeout = 15000;

        try
        {
            await clienteSmtp.ConnectAsync(opcionesSmtp.Servidor, opcionesSmtp.Puerto, SecureSocketOptions.StartTls);
            await clienteSmtp.AuthenticateAsync(opcionesSmtp.Usuario, opcionesSmtp.Contrasena);
            await clienteSmtp.SendAsync(mensajeMime);
            await clienteSmtp.DisconnectAsync(true);
        }
        catch (Exception excepcion)
        {
            registrador.LogError(excepcion, "No se pudo enviar el correo mediante SMTP.");
            throw new InvalidOperationException("No se pudo enviar el correo electrónico.", excepcion);
        }
    }

    private void ValidarConfiguracion()
    {
        if (string.IsNullOrWhiteSpace(opcionesSmtp.Servidor)
            || string.IsNullOrWhiteSpace(opcionesSmtp.Usuario)
            || string.IsNullOrWhiteSpace(opcionesSmtp.Contrasena)
            || string.IsNullOrWhiteSpace(opcionesSmtp.CorreoRemitente))
        {
            throw new InvalidOperationException("La configuración SMTP está incompleta. Revisa la sección Smtp en User Secrets.");
        }
    }
}