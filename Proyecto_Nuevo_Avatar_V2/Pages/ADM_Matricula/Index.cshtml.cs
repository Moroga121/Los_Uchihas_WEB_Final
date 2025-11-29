using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Matricula
{
    public class IndexModel : PageModel
    {

        private readonly ILoginApiClient _loginApiClient;
        private readonly IPrematriculaApiClient _prematriculaApiClient;
        private readonly IMatriculaApiClient _matriculaApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly IConfiguration _configuration;


        public IndexModel(ILoginApiClient loginApiClient, IPrematriculaApiClient prematriculaApiClient, IMatriculaApiClient matriculaApiClient, IPeriodoApiClient periodoApiClient, IConfiguration configuration)
        {
            _loginApiClient = loginApiClient;
            _prematriculaApiClient = prematriculaApiClient;
            _matriculaApiClient = matriculaApiClient;
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

        #region "Listas"

        public List<MatriculaCompletaDto> Matriculas { get; set; } = new();
        public List<Prematricula> Prematriculas { get; set; } = new();
        public List<Prematricula> PrematriculasPaginadas { get; set; } = new();

        #endregion

        [BindProperty(SupportsGet = true)]
        public string? CursoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? GrupoFiltro { get; set; }

        #region "Exportar a CSV"

        public async Task<IActionResult> OnPostExportCSVAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            // Traer TODO
            var matriculados = await _matriculaApiClient.Obtener_Todas_Matriculas(token) ?? new List<MatriculaCompletaDto>();

            // Aplicar los MISMOS filtros
            if (!string.IsNullOrWhiteSpace(CursoFiltro))
            {
                matriculados = matriculados.Where(m => m.Curso.Contains(CursoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(GrupoFiltro))
            {
                matriculados = matriculados.Where(m => m.Grupo.Contains(GrupoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // CSV
            var csv = new StringBuilder();
            csv.AppendLine("LISTADO DE MATRICULADOS");
            csv.AppendLine($"Curso;{(string.IsNullOrEmpty(CursoFiltro) ? "Todos" : CursoFiltro)};Grupo;{(string.IsNullOrEmpty(GrupoFiltro) ? "Todos" : GrupoFiltro)}");
            csv.AppendLine($"Generado;{DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            csv.AppendLine("ID Matrícula;Identificación;Nombre;Curso;Grupo");

            foreach (var m in matriculados)
            {
                csv.AppendLine($"{m.Id_Matricula};{m.Estudiante.Numero_Identificacion};{m.Estudiante.Nombre};{m.Curso};{m.Grupo}");
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
                .ToArray();

            return File(bytes, "text/csv; charset=utf-8",
                $"Matriculados_{DateTime.Now:yyyyMMddHHmmss}.csv");
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
                return RedirectToPage("/Login/Login");

            

            Prematriculas = await _prematriculaApiClient.Obtener_Todas_Prematriculas(token) ?? new List<Prematricula>();

            Matriculas = await _matriculaApiClient.Obtener_Todas_Matriculas(token) ?? new List<MatriculaCompletaDto>();

            if (!string.IsNullOrWhiteSpace(CursoFiltro))
            {
                Matriculas = Matriculas
                    .Where(m => m.Curso.Contains(CursoFiltro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(GrupoFiltro))
            {
                Matriculas = Matriculas
                    .Where(m => m.Grupo.Contains(GrupoFiltro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }



            TotalRegistros = Prematriculas.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
            if (TotalPaginas == 0) TotalPaginas = 1;

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            PrematriculasPaginadas = Prematriculas
                .Skip((PaginaActual - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();

            return Page();
        }

    }
}
