using System.Security.Claims;
using AutenticacionSeguraAA01_JC.Comunes.Constantes;
using AutenticacionSeguraAA01_JC.Negocio.Dtos;
using AutenticacionSeguraAA01_JC.Negocio.Interfaces;
using AutenticacionSeguraAA01_JC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSeguraAA01_JC.Controllers;

[Authorize(Roles = ConstantesNombresRoles.ADMINISTRADOR)]
public class AdministracionController : Controller
{
    private readonly IServicioAdministracionUsuarios servicioAdministracionUsuarios;

    public AdministracionController(IServicioAdministracionUsuarios servicioAdministracionUsuarios)
    {
        this.servicioAdministracionUsuarios = servicioAdministracionUsuarios;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dtoUsuariosListados = await servicioAdministracionUsuarios.ListarUsuariosAsync();

        var usuarioListadoViewModels = dtoUsuariosListados
            .Select(dtoUsuarioListado => new UsuarioListadoViewModel
            {
                IdUsuario = dtoUsuarioListado.IdUsuario,
                NombreCompleto = dtoUsuarioListado.NombreCompleto,
                Correo = dtoUsuarioListado.Correo,
                Rol = dtoUsuarioListado.Rol,
                Habilitado = dtoUsuarioListado.Habilitado,
                FechaRegistro = dtoUsuarioListado.FechaRegistro,
                EsUsuarioActual = dtoUsuarioListado.IdUsuario == idUsuarioActual
            })
            .ToList();

        return View(usuarioListadoViewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(CambiarRolViewModel cambiarRolViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "La solicitud no es válida.";
            return RedirectToAction(nameof(Index));
        }

        var dtoCambioRol = new DtoCambioRol
        {
            IdUsuario = cambiarRolViewModel.IdUsuario,
            NuevoRol = cambiarRolViewModel.NuevoRol
        };

        var dtoResultadoOperacion = await servicioAdministracionUsuarios.CambiarRolAsync(dtoCambioRol);

        if (dtoResultadoOperacion.Exitoso)
        {
            TempData["Mensaje"] = "El rol fue actualizado.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", dtoResultadoOperacion.Errores);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoViewModel cambiarEstadoViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "La solicitud no es válida.";
            return RedirectToAction(nameof(Index));
        }

        var dtoCambioEstado = new DtoCambioEstado
        {
            IdUsuario = cambiarEstadoViewModel.IdUsuario,
            Habilitar = cambiarEstadoViewModel.Habilitar
        };

        var dtoResultadoOperacion = await servicioAdministracionUsuarios.CambiarEstadoAsync(dtoCambioEstado);

        if (dtoResultadoOperacion.Exitoso)
        {
            TempData["Mensaje"] = cambiarEstadoViewModel.Habilitar
                ? "La cuenta fue habilitada."
                : "La cuenta fue deshabilitada.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", dtoResultadoOperacion.Errores);
        }

        return RedirectToAction(nameof(Index));
    }
}