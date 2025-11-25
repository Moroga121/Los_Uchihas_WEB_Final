using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Threading.Tasks;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN1
{
    public class DetalleFacturaModel : PageModel
    {
        private readonly IFacturaApiClient _detalleFacturasService;

        public DetalleFacturaModel(IFacturaApiClient detalleFacturasService)
        {
            _detalleFacturasService = detalleFacturasService;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public FacturasDto Factura { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/Login");
            }

            Factura = await _detalleFacturasService.ObtenerFacturaPorIdAsync(accessToken, Id);

            if (Factura == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
