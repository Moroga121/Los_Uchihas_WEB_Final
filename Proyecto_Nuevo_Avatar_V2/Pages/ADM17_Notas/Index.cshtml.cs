using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM17_Notas
{
    public class IndexModel : PageModel
    {
        private readonly ILoginApiClient _loginApiClient;
        private readonly IRubros_NotasApiClient _rubros_NotasApiClient;
        private readonly ICursoApiClient _cursoApiClient;
        private readonly IGrupoApiClient _grupoApiClient;

        public IndexModel(ILoginApiClient loginApiClient,
                          IRubros_NotasApiClient rubros_NotasApiClient,
                          ICursoApiClient cursoApiClient,
                          IGrupoApiClient grupoApiClient)
        {
            _loginApiClient = loginApiClient;
            _rubros_NotasApiClient = rubros_NotasApiClient;
            _cursoApiClient = cursoApiClient;
            _grupoApiClient = grupoApiClient;
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

        [BindProperty] public string CursoSeleccionado { get; set; }
        [BindProperty] public string GrupoSeleccionado { get; set; }
        [BindProperty] public string CedulaEstudiante { get; set; }

        #region Listas 
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();

        #endregion
        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null) return RedirectToPage("/Login/Login");

            var cursosTask = _cursoApiClient.ObtenerTodosAsync(token);

            await Task.WhenAll(cursosTask);

            // SOLO cargamos cursos
            Cursos = cursosTask.Result.Select(c => new SelectListItem
            {
                Value = c.ID_Curso,
                Text = c.Nombre
            }).ToList();

            // Grupos inicia vacío (se llenará con AJAX)
            Grupos = new List<SelectListItem>();

            return Page();
        }
        public async Task<JsonResult> OnGetGruposPorCurso(string cursoId)
        {
            var token = await GetValidAccessTokenAsync();
            var grupos = await _grupoApiClient.ObtenerTodosGruposAsync(token);

            var lista = grupos
                .Where(x => x.ID_Curso == cursoId)
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Grupo,
                    Text = g.ID_Grupo
                });

            return new JsonResult(lista);
        }



    }
}
