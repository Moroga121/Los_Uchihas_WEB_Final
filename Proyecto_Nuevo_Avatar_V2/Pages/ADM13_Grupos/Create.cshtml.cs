using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM13_Grupos
{
    public class CreateModel : PageModel
    {
        private readonly IGrupoApiClient _grupoApiClient;
        private readonly ICursoApiClient _cursoApiClient;
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(
            IGrupoApiClient grupoApiClient,
            ICursoApiClient cursoApiClient,
            IProfesorApiClient profesorApiClient,
            IPeriodoApiClient periodoApiClient,
            ILoginApiClient loginApiClient)
        {
            _grupoApiClient = grupoApiClient;
            _cursoApiClient = cursoApiClient;
            _profesorApiClient = profesorApiClient;
            _periodoApiClient = periodoApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Grupo NuevoGrupo { get; set; } = new();

        public List<Curso> Cursos { get; set; } = new();
        public List<Profesor> Profesores { get; set; } = new();
        public List<Periodo> Periodos { get; set; } = new();

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

        public async Task<IActionResult> OnGetAsync()
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

            Cursos = await _cursoApiClient.ObtenerTodosAsync(token);
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token);
            Periodos = await _periodoApiClient.ObtenerTodosAsync(token);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            Cursos = await _cursoApiClient.ObtenerTodosAsync(token);
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token);
            Periodos = await _periodoApiClient.ObtenerTodosAsync(token);

            if (NuevoGrupo.Numero_Grupo <= 0)
                ModelState.AddModelError("NuevoGrupo.Numero_Grupo", "El número de grupo debe ser mayor que 0.");

            if (!Regex.IsMatch(NuevoGrupo.Horario ?? "", @"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9: -]+$"))
                ModelState.AddModelError("NuevoGrupo.Horario", "El horario solo puede contener letras, números y los símbolos ':' '-' y espacios.");

            if (!ModelState.IsValid)
                return Page();

            var (Exito, Mensaje, _) = await _grupoApiClient.CRUDGrupoAsync(NuevoGrupo, "Insert", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}

