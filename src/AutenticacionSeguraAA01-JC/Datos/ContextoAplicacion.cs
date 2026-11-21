using AutenticacionSeguraAA01_JC.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutenticacionSeguraAA01_JC.Datos
{
    public class ContextoAplicacion : IdentityDbContext<UsuarioAplicacion>
    {
        public ContextoAplicacion(DbContextOptions<ContextoAplicacion> opcionesContexto)
            : base(opcionesContexto)
        {
        }

        protected override void OnModelCreating(ModelBuilder constructorModelo)
        {
            base.OnModelCreating(constructorModelo);

            constructorModelo.Entity<UsuarioAplicacion>(constructorEntidad =>
            {
                constructorEntidad.Property(usuarioAplicacion => usuarioAplicacion.NombreCompleto)
                    .IsRequired()
                    .HasMaxLength(100);
            });
            constructorModelo.Entity<UsuarioAplicacion>().ToTable("Usuarios");
            constructorModelo.Entity<IdentityRole>().ToTable("Roles");
            constructorModelo.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
            constructorModelo.Entity<IdentityUserClaim<string>>().ToTable("UsuariosReclamos");
            constructorModelo.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
            constructorModelo.Entity<IdentityRoleClaim<string>>().ToTable("RolesReclamos");
            constructorModelo.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");
        }
    }
}
