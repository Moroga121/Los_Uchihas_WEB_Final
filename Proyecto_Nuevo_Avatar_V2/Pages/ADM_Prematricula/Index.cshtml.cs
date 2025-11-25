using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Prematricula
{
    public class IndexModel : PageModel
    {
        private readonly ILoginApiClient _loginApiClient;
        private readonly IPrematriculaApiClient _prematriculaApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly IConfiguration _configuration;


        public IndexModel(ILoginApiClient loginApiClient, IPrematriculaApiClient prematriculaApiClient,IPeriodoApiClient periodoApiClient, IConfiguration configuration)
        {
            _loginApiClient = loginApiClient;
            _prematriculaApiClient = prematriculaApiClient;
            _periodoApiClient = periodoApiClient;
            _configuration = configuration;
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

        #region Listas 
        public List<Prematricula> PrematriculaPaginadas { get; set; } = new();
        public List<Prematricula> Prematriculas { get; set; } = new();
        public List<Periodo> Periodos { get; set; } = new();

        #endregion
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


            var listaprematriculas = await _prematriculaApiClient.Obtener_Todas_Prematriculas (token);
            if (listaprematriculas != null)
            {
                Prematriculas = listaprematriculas;

                TotalRegistros = listaprematriculas.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

                // Validar y ajustar el número de página

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                // Aplicar paginación
                PrematriculaPaginadas = Prematriculas.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

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


            var prematriculaeliminar = await _prematriculaApiClient.ObtenerPrematriculaPorId(id, token);
            if (prematriculaeliminar == null)
            {
                TempData["Resultado"] = "La prematricula no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _prematriculaApiClient.CRUDPrematricula(prematriculaeliminar, token, "Delete");

            if (Exito)
            {
                TempData["Resultado"] = "Prematricula eliminado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = $"Error al eliminar la Prematricula: {Mensaje}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToPage();
        }
    }
}
