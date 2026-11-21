using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSeguraAA01_JC.Controllers;

public class CuentaController : Controller
{
    private readonly IServicioAutenticacion servicioAutenticacion;
    private readonly IServicioRecuperacionCuenta servicioRecuperacionCuenta;

    public CuentaController(
        IServicioAutenticacion servicioAutenticacion,
        IServicioRecuperacionCuenta servicioRecuperacionCuenta)
    {
        this.servicioAutenticacion = servicioAutenticacion;
        this.servicioRecuperacionCuenta = servicioRecuperacionCuenta;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Registrar()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(AreaPersonal));
        }

        return View(new RegistroViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel registroViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(registroViewModel);
        }

        var dtoRegistro = new DtoRegistro
        {
            NombreCompleto = registroViewModel.NombreCompleto,
            Correo = registroViewModel.Correo,
            Contrasena = registroViewModel.Contrasena
        };

        var dtoResultadoOperacion = await servicioAutenticacion.RegistrarAsync(dtoRegistro);

        if (!dtoResultadoOperacion.Exitoso)
        {
            foreach (var error in dtoResultadoOperacion.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            registroViewModel.Contrasena = string.Empty;
            registroViewModel.ConfirmarContrasena = string.Empty;
            return View(registroViewModel);
        }

        TempData["Mensaje"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(IniciarSesion));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(AreaPersonal));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new IngresoViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(IngresoViewModel ingresoViewModel, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            ingresoViewModel.Contrasena = string.Empty;
            return View(ingresoViewModel);
        }

        var dtoIngreso = new DtoIngreso
        {
            Correo = ingresoViewModel.Correo,
            Contrasena = ingresoViewModel.Contrasena,
            Recordarme = ingresoViewModel.Recordarme
        };

        var dtoResultadoIngreso = await servicioAutenticacion.IniciarSesionAsync(dtoIngreso);

        if (!dtoResultadoIngreso.Exitoso)
        {
            ModelState.AddModelError(string.Empty, dtoResultadoIngreso.Mensaje);
            ingresoViewModel.Contrasena = string.Empty;
            return View(ingresoViewModel);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(AreaPersonal));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await servicioAutenticacion.CerrarSesionAsync();
        return RedirectToAction(nameof(IniciarSesion));
    }

    [Authorize]
    [HttpGet]
    public IActionResult AreaPersonal()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult OlvideContrasena()
    {
        return View(new OlvideContrasenaViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel olvideContrasenaViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(olvideContrasenaViewModel);
        }

        var urlBaseRestablecimiento = Url.Action(nameof(RestablecerContrasena), "Cuenta", null, Request.Scheme)!;

        var dtoSolicitudRecuperacion = new DtoSolicitudRecuperacion
        {
            Correo = olvideContrasenaViewModel.Correo,
            UrlBaseRestablecimiento = urlBaseRestablecimiento
        };

        await servicioRecuperacionCuenta.SolicitarRecuperacionAsync(dtoSolicitudRecuperacion);

        TempData["Mensaje"] = "Si el correo está registrado, te enviamos un enlace para restablecer tu contraseña. Revisa tu bandeja de entrada y la carpeta de spam.";
        return RedirectToAction(nameof(IniciarSesion));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> RestablecerContrasena(string? idUsuario, string? token)
    {
        var enlaceValido = await servicioRecuperacionCuenta.ValidarEnlaceAsync(idUsuario ?? string.Empty, token ?? string.Empty);

        if (!enlaceValido)
        {
            return View("EnlaceInvalido");
        }

        var restablecerContrasenaViewModel = new RestablecerContrasenaViewModel
        {
            IdUsuario = idUsuario!,
            Token = token!
        };

        return View(restablecerContrasenaViewModel);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel restablecerContrasenaViewModel)
    {
        if (!ModelState.IsValid)
        {
            restablecerContrasenaViewModel.NuevaContrasena = string.Empty;
            restablecerContrasenaViewModel.ConfirmarContrasena = string.Empty;
            return View(restablecerContrasenaViewModel);
        }

        var dtoRestablecimiento = new DtoRestablecimiento
        {
            IdUsuario = restablecerContrasenaViewModel.IdUsuario,
            Token = restablecerContrasenaViewModel.Token,
            NuevaContrasena = restablecerContrasenaViewModel.NuevaContrasena
        };

        var dtoResultadoOperacion = await servicioRecuperacionCuenta.RestablecerContrasenaAsync(dtoRestablecimiento);

        if (!dtoResultadoOperacion.Exitoso)
        {
            foreach (var error in dtoResultadoOperacion.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            restablecerContrasenaViewModel.NuevaContrasena = string.Empty;
            restablecerContrasenaViewModel.ConfirmarContrasena = string.Empty;
            return View(restablecerContrasenaViewModel);
        }

        TempData["Mensaje"] = "Tu contraseña fue actualizada. Ya puedes iniciar sesión con la nueva.";
        return RedirectToAction(nameof(IniciarSesion));
    }
}