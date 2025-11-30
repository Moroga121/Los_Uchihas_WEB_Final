using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Expedientes
{
    public class IndexModel : PageModel
    {
        private readonly IExpedientesApiClient _expedientesApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IExpedientesApiClient expedientesApiClient, ILoginApiClient loginApiClient, IConfiguration configuration)
        {
            _expedientesApiClient = expedientesApiClient;
            _loginApiClient = loginApiClient;
            _configuration = configuration;
        }

        #region Paginación

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        #endregion

        #region Validar Token

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

        #region Propiedades

        public List<ExpedienteDto> Expedientes { get; set; } = new();
        

        #endregion

        
        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            var lista = await _expedientesApiClient.Obtener_Todos_Expedientes(token);

            if (lista != null)
            {
                Expedientes = lista;

                TotalRegistros = Expedientes.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                Expedientes = Expedientes.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();
            }

            return Page();
        }

        
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var expediente = await _expedientesApiClient.ObtenerExpedientePorId(id, token);

            if (expediente == null)
            {
                TempData["Resultado"] = "El expediente no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _expedientesApiClient.CRUDExpedientes(expediente, token, "Delete");

            if (Exito)
            {
                TempData["Resultado"] = "Expediente eliminado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = $"Error al eliminar el expediente: {Mensaje}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToPage();
        }
    }
}
