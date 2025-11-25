using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM10_Cursos
{
    public class IndexModel : PageModel
    {
        private readonly ICursoApiClient _cursoApiClient;
        private readonly ICarreraApiClient _carreraApiClient;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(ICursoApiClient cursoApiClient, ICarreraApiClient carreraApiClient, IConfiguration confi, ILoginApiClient loginApiClient)
        {
            _cursoApiClient = cursoApiClient;
            _carreraApiClient = carreraApiClient;
            _configuration = confi;
            _loginApiClient = loginApiClient;
        }

        public List<Curso>? Cursos { get; set; }
        public List<Carrera>? Carreras { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CarreraSeleccionada { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? NivelFiltro { get; set; }

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

            Carreras = await _carreraApiClient.ObtenerTodasCarrerasAsync(token);
            var cursos = await _cursoApiClient.ObtenerTodosAsync(token);

            if (!string.IsNullOrEmpty(CarreraSeleccionada))
                cursos = cursos.Where(c => c.ID_Carrera == CarreraSeleccionada).ToList();

            if (NivelFiltro.HasValue && NivelFiltro > 0)
                cursos = cursos.Where(c => c.Nivel == NivelFiltro).ToList();

            foreach (var curso in cursos)
            {
                var carrera = Carreras.FirstOrDefault(c => c.ID_Carrera == curso.ID_Carrera);
                if (carrera != null)
                {
                    curso.Mensaje = carrera.Nombre; 
                }
            }

            Cursos = cursos;

            // Paginación

            TotalRegistros = Cursos.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Cursos = Cursos.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

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

            var curso = new Curso { ID_Curso = id };
            var (Exito, Mensaje, _) = await _cursoApiClient.CRUDCursoAsync(curso, "Delete", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }
    }
}
