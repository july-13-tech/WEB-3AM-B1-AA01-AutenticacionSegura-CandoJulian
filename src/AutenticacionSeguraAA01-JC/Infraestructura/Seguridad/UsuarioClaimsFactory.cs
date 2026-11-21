using AutenticacionSeguraAA01_JC.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AutenticacionSeguraAA01_JC.Infraestructura
{
    public class UsuarioClaimsFactory : UserClaimsPrincipalFactory<UsuarioAplicacion>
    {
        private readonly ILogger<UsuarioClaimsFactory> _logger;

        public UsuarioClaimsFactory(
            UserManager<UsuarioAplicacion> userManager,
            IOptions<IdentityOptions> options,
            ILogger<UsuarioClaimsFactory> logger)
            : base(userManager, options)
        {
            _logger = logger;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(
            UsuarioAplicacion user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            _logger.LogWarning(
                "ClaimsFactory ejecutada. Usuario: {Usuario}, NombreCompleto: {Nombre}",
                user.UserName,
                user.NombreCompleto);

            identity.AddClaim(
                new Claim("NombreCompleto", user.NombreCompleto ?? string.Empty));

            _logger.LogWarning(
                "Claim agregado: {Claim}",
                identity.FindFirst("NombreCompleto")?.Value);

            return identity;
        }
    }
}