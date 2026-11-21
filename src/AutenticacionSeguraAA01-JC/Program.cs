using AutenticacionSeguraAA01_JC.Datos;
using AutenticacionSeguraAA01_JC.Datos.Entidades;
using AutenticacionSeguraAA01_JC.Datos.Seed;
using AutenticacionSeguraAA01_JC.Infraestructura.Correo;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextoAplicacion>(opcionesContexto =>
    opcionesContexto.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<UsuarioAplicacion, IdentityRole>(opcionesIdentidad =>
    {
        opcionesIdentidad.Password.RequiredLength = 8;
        opcionesIdentidad.Password.RequireDigit = true;
        opcionesIdentidad.Password.RequireLowercase = true;
        opcionesIdentidad.Password.RequireUppercase = true;
        opcionesIdentidad.Password.RequireNonAlphanumeric = true;

        opcionesIdentidad.User.RequireUniqueEmail = true;

        opcionesIdentidad.Lockout.AllowedForNewUsers = true;
        opcionesIdentidad.Lockout.MaxFailedAccessAttempts = 5;
        opcionesIdentidad.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddErrorDescriber<DescriptorErroresIdentidad>()
    .AddEntityFrameworkStores<ContextoAplicacion>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(opcionesValidador =>
{
    opcionesValidador.ValidationInterval = TimeSpan.Zero;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(opcionesToken =>
{
    opcionesToken.TokenLifespan = TimeSpan.FromMinutes(15);
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

builder.Services.AddAntiforgery(opcionesAntiforgery =>
{
    opcionesAntiforgery.Cookie.Name = "AutenticacionSegura.Antiforgery";
    opcionesAntiforgery.Cookie.HttpOnly = true;
    opcionesAntiforgery.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization(opcionesAutorizacion =>
{
    opcionesAutorizacion.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.Configure<OpcionesAdministrador>(
    builder.Configuration.GetSection(OpcionesAdministrador.NombreSeccion));

builder.Services.Configure<OpcionesSmtp>(
    builder.Configuration.GetSection(OpcionesSmtp.NombreSeccion));

builder.Services.AddScoped<InicializadorIdentidad>();
builder.Services.AddScoped<IEmailService, ServicioCorreoSmtp>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IServicioRecuperacionCuenta, ServicioRecuperacionCuenta>();
builder.Services.AddScoped<IServicioPerfil, ServicioPerfil>();
builder.Services.AddScoped<IServicioAdministracionUsuarios, ServicioAdministracionUsuarios>();

builder.Services.AddControllersWithViews(opcionesMvc =>
{
    opcionesMvc.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

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

app.Use(async (contextoHttp, siguiente) =>
{
    contextoHttp.Response.Headers["X-Content-Type-Options"] = "nosniff";
    contextoHttp.Response.Headers["X-Frame-Options"] = "DENY";
    contextoHttp.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    contextoHttp.Response.OnStarting(() =>
    {
        var tipoContenido = contextoHttp.Response.ContentType;

        if (contextoHttp.User.Identity?.IsAuthenticated == true
            && tipoContenido is not null
            && tipoContenido.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
        {
            contextoHttp.Response.Headers.CacheControl = "no-store";
        }

        return Task.CompletedTask;
    });

    await siguiente();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();