using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN1
{
    public class IndexModel : PageModel
    {
        private readonly IFacturaApiClient _facturasService;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;
        public IndexModel(IFacturaApiClient facturasService, IConfiguration confi, ILoginApiClient loginApiClient)
        {
            _facturasService = facturasService;
            _configuration = confi;
            _loginApiClient = loginApiClient;
        }

        public List<FacturasDto> Facturas { get; set; } = new();

        [TempData]
        public string ModalTitle { get; set; }

        [TempData]
        public string ModalMessage { get; set; }

        [TempData]
        public string ModalType { get; set; }


        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroPeriodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroEstudiante { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroEstado { get; set; }

        #region "Validar Token"

        private async Task<string?> GetValidAccessTokenAsync()
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var refreshToken = HttpContext.Session.GetString("RefreshToken");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return null;

            bool valido = await _loginApiClient.ValidateTokenAsync(accessToken);
            if (!valido)
            {
                var nuevoToken = await _loginApiClient.RefreshTokenAsync(refreshToken);
                if (nuevoToken != null)
                {
                    HttpContext.Session.SetString("AccessToken", nuevoToken.Access_Token);
                    HttpContext.Session.SetString("RefreshToken", nuevoToken.Refresh_Token);
                    HttpContext.Session.SetString("Expiresin", nuevoToken.Expires_In.ToString());
                    accessToken = nuevoToken.Access_Token;
                }
                else
                {
                    return null;
                }
            }

            return accessToken;
        }

        #endregion
        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            try
            {
                var facturas = await _facturasService.ObtenerFacturasAsync(token);

                if (facturas == null || !facturas.Any())
                {
                    Facturas = new List<FacturasDto>();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(FiltroPeriodo))
                        facturas = facturas.Where(f => f.Periodo.Equals(FiltroPeriodo, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (!string.IsNullOrWhiteSpace(FiltroEstudiante))
                        facturas = facturas.Where(f => f.Identificacion.Equals(FiltroEstudiante, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (!string.IsNullOrWhiteSpace(FiltroEstado))
                        facturas = facturas.Where(f => f.Estado_Factura.Equals(FiltroEstado, StringComparison.OrdinalIgnoreCase)).ToList();

                    TotalRegistros = facturas.Count;
                    TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                    if (TotalPaginas <= 0) TotalPaginas = 1;

                    PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                    PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                    Facturas = facturas
                        .Skip((PaginaActual - 1) * TamanoPagina)
                        .Take(TamanoPagina)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModalTitle = "Error";
                ModalMessage = "Error al cargar las facturas";
                ModalType = "error";
                Facturas = new List<FacturasDto>();
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            return Page();
        }


        public async Task<IActionResult> OnPostReversarFacturaAsync(long NumeroFactura, string Motivo)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                ModalTitle = "Error";
                ModalMessage = "El motivo es requerido";
                ModalType = "error";
                return RedirectToPage();
            }

            try
            {
                var resultado = await _facturasService.ReversarFacturaAsync(token, NumeroFactura, Motivo);

                if (resultado != null)
                {
                    ModalTitle = "Éxito";
                    ModalMessage = $"La factura #{NumeroFactura} ha sido anulada correctamente";
                    ModalType = "success";
                }
                else
                {
                    ModalTitle = "Error";
                    ModalMessage = "No se pudo anular la factura";
                    ModalType = "error";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModalTitle = "Error";
                ModalMessage = $"Error al anular la factura: {ex.Message}";
                ModalType = "error";
            }

            return RedirectToPage();
        }
    }
}
