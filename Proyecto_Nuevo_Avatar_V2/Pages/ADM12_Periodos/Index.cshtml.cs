using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM12_Periodos
{
    public class IndexModel : PageModel
    {
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(IPeriodoApiClient periodoApiClient, IConfiguration configuration, ILoginApiClient loginApiClient)
        {
            _periodoApiClient = periodoApiClient;
            _configuration = configuration;
            _loginApiClient = loginApiClient;
        }

        public List<Periodo>? Periodos { get; set; }

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

        #region "Paginación"

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        #endregion

        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            Periodos = await _periodoApiClient.ObtenerTodosAsync(token);

            if (Periodos != null && Periodos.Any())
            {
                foreach (var p in Periodos)
                {
                    if (DateTime.Today < p.Fecha_Inicio)
                        p.Estado = "Futuro";
                    else if (DateTime.Today >= p.Fecha_Inicio && DateTime.Today <= p.Fecha_Fin)
                        p.Estado = "Activo";
                    else
                        p.Estado = "Cerrado";
                }
            }

            // Paginación

            TotalRegistros = Periodos.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Periodos = Periodos.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var periodo = new Periodo { ID_Periodo = id };
            var (Exito, Mensaje, _) = await _periodoApiClient.CRUDPeriodoAsync(periodo, "Delete", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }
    }
}
