using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Bitacora
{
    public class BitacoraModel : PageModel
    {

        private readonly IBitacoraApiClient _bitacoraApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public BitacoraModel(IBitacoraApiClient bitacoraApiClient, ILoginApiClient loginApiClient, IConfiguration configuration)
        {
            _bitacoraApiClient = bitacoraApiClient;
            _loginApiClient = loginApiClient;
            _configuration = configuration;
        }

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

        public List<BitacoraDto> Bitacoras { get; set; } = new();

        #region "Filtros de búsqueda"

        [BindProperty(SupportsGet = true)] public DateOnly? FiltroFechaInicio { get; set; }
        [BindProperty(SupportsGet = true)] public DateOnly? FiltroFechaFinal { get; set; }
        [BindProperty(SupportsGet = true)] public string? FiltroUsuario { get; set; }
        [BindProperty(SupportsGet = true)] public string? FiltroAccion { get; set; }

        #endregion

        #region "Paginación"

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

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
                return RedirectToPage("/Login/Login");
            }

            try
            {
                // Fechas válidas (evita 01/01/0001)
                DateOnly? fechaInicioValida = (FiltroFechaInicio.HasValue && FiltroFechaInicio.Value > new DateOnly(1900, 1, 1))
                    ? FiltroFechaInicio
                    : null;

                DateOnly? fechaFinValida = (FiltroFechaFinal.HasValue && FiltroFechaFinal.Value > new DateOnly(1900, 1, 1))
                    ? FiltroFechaFinal
                    : null;

                bool hayFiltros =
                    fechaInicioValida != null ||
                    fechaFinValida != null ||
                    !string.IsNullOrWhiteSpace(FiltroUsuario) ||
                    !string.IsNullOrWhiteSpace(FiltroAccion);

                if (hayFiltros)
                {
                    Bitacoras = await _bitacoraApiClient.ObtenerBitacorasAsyncFiltradas(
                        token,
                        fechaInicioValida,
                        fechaFinValida,
                        FiltroUsuario,
                        FiltroAccion
                    ) ?? new List<BitacoraDto>();
                }
                else
                {
                    Bitacoras = await _bitacoraApiClient.ObtenerBitacorasAsync(token)
                                 ?? new List<BitacoraDto>();
                }

                // Paginación
                TotalRegistros = Bitacoras.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

                PaginaActual = Math.Clamp(numeroPagina, 1, TotalPaginas == 0 ? 1 : TotalPaginas);
                Bitacoras = Bitacoras.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las bitácoras: {ex.Message}";
            }

            return Page();
        }

    }
}
