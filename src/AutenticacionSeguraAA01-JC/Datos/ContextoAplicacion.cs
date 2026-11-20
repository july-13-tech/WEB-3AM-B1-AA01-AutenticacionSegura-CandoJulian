using AutenticacionSeguraAA01_JC.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutenticacionSeguraAA01_JC.Datos
{
    public class ContextoAplicacion : IdentityDbContext<UsuarioAplicacion>
    {
        public ContextoAplicacion(DbContextOptions<ContextoAplicacion> opciones)
            : base(opciones)
        {
        }

        protected override void OnModelCreating(ModelBuilder constructor)
        {
            base.OnModelCreating(constructor);

            constructor.Entity<UsuarioAplicacion>(entidad =>
            {
                entidad.Property(u => u.NombreCompleto)
                       .IsRequired()
                       .HasMaxLength(100);
            });
            constructor.Entity<UsuarioAplicacion>().ToTable("Usuarios");
            constructor.Entity<IdentityRole>().ToTable("Roles");
            constructor.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
            constructor.Entity<IdentityUserClaim<string>>().ToTable("UsuariosReclamos");
            constructor.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
            constructor.Entity<IdentityRoleClaim<string>>().ToTable("RolesReclamos");
            constructor.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");
        }
    }
}
