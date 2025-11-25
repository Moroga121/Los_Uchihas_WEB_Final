using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Reflection;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Modulos
{
    public class EditModel : PageModel
    {
        private readonly IModuloApiClient _modulosApiClient;

        public EditModel(IModuloApiClient modulosApiClient)
        {
            _modulosApiClient = modulosApiClient;
        }

        [BindProperty]
        public Modulo ModuloEditado { get; set; } = new();
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Activo", Text = "Activo" },
            new SelectListItem { Value = "Inactivo", Text = "Inactivo" }
        };
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            var modulo = await _modulosApiClient.ObtenerModuloPorId(id, token);
            if (modulo == null)
                return RedirectToPage("Index");

            ModuloEditado = modulo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            var (Exito, Mensaje, moduloEditado) = await _modulosApiClient.CRUDModulos(ModuloEditado, token, "Update");

            TempData["Resultado"] = Exito ? "Modulo actualizado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}
