using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Linq;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM11_Profesores
{
    public class IndexModel : PageModel
    {
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(IProfesorApiClient profesorApiClient, IConfiguration confi, ILoginApiClient loginApiClient)
        {
            _profesorApiClient = profesorApiClient;
            _configuration = confi;
            _loginApiClient = loginApiClient;
        }

        public List<Profesor> Profesores { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

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

            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token) ?? new List<Profesor>();

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                Busqueda = Busqueda.Trim();
                Profesores = Profesores
                    .Where(p =>
                        (p.Nombre != null && p.Nombre.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Email != null && p.Email.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (p.NumeroIdentificacion != null && p.NumeroIdentificacion.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            // Paginación

            TotalRegistros = Profesores.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Profesores = Profesores.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

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

            var profesor = new Profesor { ID_Profesor = id };
            var (Exito, Mensaje, _) = await _profesorApiClient.CRUDProfesorAsync(profesor, "Delete", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }
    }
}

