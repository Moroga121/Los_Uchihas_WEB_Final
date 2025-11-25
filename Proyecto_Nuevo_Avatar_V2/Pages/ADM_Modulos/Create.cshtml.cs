using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Modulos
{
    public class CreateModel : PageModel
    {
        private readonly IModuloApiClient _modulosApiClient;

        public CreateModel(IModuloApiClient modulosApiClient)
        {
            _modulosApiClient = modulosApiClient;
        }

        [BindProperty]
        public Modulo NuevoModulo { get; set; } = new();

        public void OnGet()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            var nuevomodulo = new Modulo
            {
                Identificador_Modulo = NuevoModulo.Identificador_Modulo,
                Nombre_Modulo = NuevoModulo.Nombre_Modulo,
                Estado = NuevoModulo.Estado,
                Orden = NuevoModulo.Orden
            };

            var (Exito, Mensaje, Rol) = await _modulosApiClient.CRUDModulos(nuevomodulo, token, "Insert");

            TempData["Resultado"] = Exito ? "Modulo creado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();

        }
    }
}
