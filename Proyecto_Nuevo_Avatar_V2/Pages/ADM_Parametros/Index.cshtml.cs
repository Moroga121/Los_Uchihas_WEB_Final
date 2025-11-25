using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Parametros
{
    public class IndexModel : PageModel
    {

        private readonly IParametrosApiClient _parametroApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IParametrosApiClient parametroApiClient, ILoginApiClient loginApiClient, IConfiguration confi)
        {
            _parametroApiClient = parametroApiClient;
            _loginApiClient = loginApiClient;
            _configuration = confi;
        }

        #region "Paginación"

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        #endregion

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


        public List<Parametrizacion> ParametrosPaginados { get; set; } = new();
        public List<Parametrizacion> Parametros { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            // Cargar configuración de paginación desde appsettings

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            var token = await GetValidAccessTokenAsync();

            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }


            var lista = await _parametroApiClient.ObtenerParametrosAsync(token);
            if (lista != null)
            {
                Parametros = lista;

                TotalRegistros = lista.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

                // Validar y ajustar el número de página

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                // Aplicar paginación
                ParametrosPaginados = Parametros.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            }

            return Page();

        }
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();

            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");


            var parametroeliminar = await _parametroApiClient.ObtenerParametrolPorId(id, token);
            if (parametroeliminar == null)
            {
                TempData["Resultado"] = "El Parámetro no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _parametroApiClient.CRUDParametros(parametroeliminar, token, "Delete");

            if (Exito)
            {
                TempData["Resultado"] = "Parámetro eliminado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = $"Error al eliminar el Parámetro: {Mensaje}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToPage();
        }
    }
}
