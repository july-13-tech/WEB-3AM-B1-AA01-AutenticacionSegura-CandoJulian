using Microsoft.AspNetCore.Identity;

namespace AutenticacionSeguraAA01_JC.Datos.Entidades
{
    public class UsuarioAplicacion : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}