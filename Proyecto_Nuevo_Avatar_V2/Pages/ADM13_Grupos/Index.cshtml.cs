using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Linq;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM13_Grupos
{
    public class IndexModel : PageModel
    {
        private readonly IGrupoApiClient _grupoApiClient;
        private readonly ICursoApiClient _cursoApiClient;
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(
            IGrupoApiClient grupoApiClient,
            ICursoApiClient cursoApiClient,
            IProfesorApiClient profesorApiClient,
            IPeriodoApiClient periodoApiClient,
            IConfiguration configuration,
            ILoginApiClient loginApiClient)
        {
            _grupoApiClient = grupoApiClient;
            _cursoApiClient = cursoApiClient;
            _profesorApiClient = profesorApiClient;
            _periodoApiClient = periodoApiClient;
            _configuration = configuration;
            _loginApiClient = loginApiClient;
        }

        public List<Grupo> Grupos { get; set; } = new();
        public List<Curso> Cursos { get; set; } = new();
        public List<Profesor> Profesores { get; set; } = new();
        public List<Periodo> Periodos { get; set; } = new();

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

            Grupos = await _grupoApiClient.ObtenerTodosGruposAsync(token) ?? new List<Grupo>();
            Cursos = await _cursoApiClient.ObtenerTodosAsync(token) ?? new List<Curso>();
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token) ?? new List<Profesor>();
            Periodos = await _periodoApiClient.ObtenerTodosAsync(token) ?? new List<Periodo>();

            foreach (var grupo in Grupos)
            {
                grupo.ID_Curso = Cursos.FirstOrDefault(c => c.ID_Curso == grupo.ID_Curso)?.Nombre ?? grupo.ID_Curso;
                grupo.ID_Profesor = Profesores.FirstOrDefault(p => p.ID_Profesor == grupo.ID_Profesor)?.Nombre ?? grupo.ID_Profesor;
                grupo.ID_Periodo = Periodos.FirstOrDefault(p => p.ID_Periodo == grupo.ID_Periodo)?.ID_Periodo ?? grupo.ID_Periodo;
            }

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                Busqueda = Busqueda.Trim();
                Grupos = Grupos
                    .Where(g =>
                        g.ID_Curso.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ||
                        g.ID_Profesor.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Paginación

            TotalRegistros = Grupos.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Grupos = Grupos.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

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

            var grupo = new Grupo { ID_Grupo = id };
            var (Exito, Mensaje, _) = await _grupoApiClient.CRUDGrupoAsync(grupo, "Delete", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }
    }
}

