using AutenticacionSeguraAA01_JC.Datos;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutenticacionSeguraAA01_JC.Datos.Seed;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextoAplicacion>(opcionesContexto =>
    opcionesContexto.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<UsuarioAplicacion, IdentityRole>(opcionesIdentidad =>
    {
        opcionesIdentidad.Password.RequiredLength = 10;
        opcionesIdentidad.Password.RequireDigit = true;
        opcionesIdentidad.Password.RequireLowercase = true;
        opcionesIdentidad.Password.RequireUppercase = true;
        opcionesIdentidad.Password.RequireNonAlphanumeric = true;

        opcionesIdentidad.User.RequireUniqueEmail = true;

        opcionesIdentidad.Lockout.AllowedForNewUsers = true;
        opcionesIdentidad.Lockout.MaxFailedAccessAttempts = 3;
        opcionesIdentidad.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    })
    .AddErrorDescriber<DescriptorErroresIdentidad>()
    .AddEntityFrameworkStores<ContextoAplicacion>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opcionesToken =>
{
    opcionesToken.TokenLifespan = TimeSpan.FromMinutes(2);
});

builder.Services.ConfigureApplicationCookie(opcionesCookie =>
{
    opcionesCookie.Cookie.Name = "AutenticacionSegura.Auth";
    opcionesCookie.Cookie.HttpOnly = true;
    opcionesCookie.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opcionesCookie.Cookie.SameSite = SameSiteMode.Lax;

    opcionesCookie.LoginPath = "/Cuenta/IniciarSesion";
    opcionesCookie.LogoutPath = "/Cuenta/CerrarSesion";
    opcionesCookie.AccessDeniedPath = "/Cuenta/AccesoDenegado";

    opcionesCookie.ExpireTimeSpan = TimeSpan.FromDays(7);
    opcionesCookie.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<OpcionesAdministrador>(
    builder.Configuration.GetSection(OpcionesAdministrador.NombreSeccion));

builder.Services.AddScoped<InicializadorIdentidad>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
var app = builder.Build();

using (var ambitoServicios = app.Services.CreateScope())
{
    var inicializadorIdentidad = ambitoServicios.ServiceProvider.GetRequiredService<InicializadorIdentidad>();
    await inicializadorIdentidad.InicializarAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();