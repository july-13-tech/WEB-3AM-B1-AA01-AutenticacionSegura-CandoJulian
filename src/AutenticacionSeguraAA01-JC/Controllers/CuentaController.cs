using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSeguraAA01_JC.Controllers;

public class CuentaController : Controller
{
    private readonly IServicioAutenticacion servicioAutenticacion;

    public CuentaController(IServicioAutenticacion servicioAutenticacion)
    {
        this.servicioAutenticacion = servicioAutenticacion;
    }

    [HttpGet]
    public IActionResult Registrar()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(AreaPersonal));
        }

        return View(new RegistroViewModel());
    }

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

            return View(registroViewModel);
        }

        TempData["Mensaje"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(IniciarSesion));
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(IngresoViewModel ingresoViewModel, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
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

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}