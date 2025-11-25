using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN2
{
    public class DetallePagoModel : PageModel
    {
        private readonly IPagoApiClient _pagoService;

        public DetallePagoModel(IPagoApiClient pagoService)
        {
            _pagoService = pagoService;
        }

        public Pago Pago { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var accessToken = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                Pago = await _pagoService.ObtenerPagoPorNumeroAsync(accessToken, id);

                if (Pago == null)
                {
                    return RedirectToPage("/IPN2/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle del pago: {ex.Message}");
                return RedirectToPage("/IPN2/Index");
            }
        }
    }
}
