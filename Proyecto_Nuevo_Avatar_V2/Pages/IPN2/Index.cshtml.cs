using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN2
{
    public class IndexModel : PageModel
    {
        private readonly IPagoApiClient _pagosService;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;
        public IndexModel(IPagoApiClient pagosService, IConfiguration confi, ILoginApiClient loginApiClient)
        {
            _pagosService = pagosService;
            _configuration = confi;
            _loginApiClient = loginApiClient;
        }

        public List<Pago> Pago { get; set; } = new();

        public string ModalTitle { get; set; }
        public string ModalMessage { get; set; }
        public string ModalType { get; set; }

        [TempData]
        public string TempModalTitle { get; set; }

        [TempData]
        public string TempModalMessage { get; set; }

        [TempData]
        public string TempModalType { get; set; }

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        [BindProperty(SupportsGet = true)]
        public string filtroPeriodo { get; set; }

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

            if (!string.IsNullOrEmpty(TempModalMessage))
            {
                ModalTitle = TempModalTitle;
                ModalMessage = TempModalMessage;
                ModalType = TempModalType;
            }

            try
            {
                var pago = await _pagosService.ObtenerPagoFacturasAsync(token);

                if (!string.IsNullOrEmpty(filtroPeriodo))
                {
                    pago = pago.Where(p => p.Periodo.Equals(filtroPeriodo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                TotalRegistros = pago.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                if (TotalPaginas <= 0) TotalPaginas = 1;

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                Pago = pago
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModalTitle = "Error";
                ModalMessage = "Error al cargar los pagos";
                ModalType = "error";
                Pago = new List<Pago>();
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostReversarPagoAsync(int NumeroPago, string Motivo)
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
                await OnGetAsync();
                return Page();
            }

            try
            {
                var resultado = await _pagosService.ReversarPagoAsync(token, NumeroPago, Motivo);

                if (resultado != null)
                {
                    TempModalTitle = "Éxito";
                    TempModalMessage = $"El pago #{NumeroPago} ha sido reversado correctamente";
                    TempModalType = "success";
                }
                else
                {
                    TempModalTitle = "Error";
                    TempModalMessage = "No se pudo reversar el pago";
                    TempModalType = "error";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempModalTitle = "Error";
                TempModalMessage = $"Error al reversar el pago: {ex.Message}";
                TempModalType = "error";
            }

            return RedirectToPage();
        }
    }
}
