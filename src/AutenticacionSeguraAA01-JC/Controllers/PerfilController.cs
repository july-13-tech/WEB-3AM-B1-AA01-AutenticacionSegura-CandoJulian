using System.Security.Claims;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSeguraAA01_JC.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly IServicioPerfil servicioPerfil;

    public PerfilController(IServicioPerfil servicioPerfil)
    {
        this.servicioPerfil = servicioPerfil;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idUsuario))
        {
            return Forbid();
        }

        var dtoPerfilUsuario = await servicioPerfil.ObtenerPerfilAsync(idUsuario);

        if (dtoPerfilUsuario is null)
        {
            return Forbid();
        }

        var perfilViewModel = new PerfilViewModel
        {
            NombreCompleto = dtoPerfilUsuario.NombreCompleto,
            Correo = dtoPerfilUsuario.Correo,
            Rol = dtoPerfilUsuario.Rol,
            Habilitado = dtoPerfilUsuario.Habilitado,
            FechaRegistro = dtoPerfilUsuario.FechaRegistro
        };

        return View(perfilViewModel);
    }

    [HttpGet]
    public IActionResult CambiarContrasena()
    {
        return View(new CambiarContrasenaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel cambiarContrasenaViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(cambiarContrasenaViewModel);
        }

        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idUsuario))
        {
            return Forbid();
        }

        var dtoCambioContrasena = new DtoCambioContrasena
        {
            IdUsuario = idUsuario,
            ContrasenaActual = cambiarContrasenaViewModel.ContrasenaActual,
            NuevaContrasena = cambiarContrasenaViewModel.NuevaContrasena
        };

        var dtoResultadoOperacion = await servicioPerfil.CambiarContrasenaAsync(dtoCambioContrasena);

        if (!dtoResultadoOperacion.Exitoso)
        {
            foreach (var error in dtoResultadoOperacion.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            cambiarContrasenaViewModel.ContrasenaActual = string.Empty;
            cambiarContrasenaViewModel.NuevaContrasena = string.Empty;
            cambiarContrasenaViewModel.ConfirmarContrasena = string.Empty;
            return View(cambiarContrasenaViewModel);
        }

        TempData["Mensaje"] = "Tu contraseña fue actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}