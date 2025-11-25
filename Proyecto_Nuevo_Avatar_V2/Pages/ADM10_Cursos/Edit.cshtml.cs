using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM10_Cursos
{
    public class EditModel : PageModel
    {
        private readonly ICursoApiClient _cursoApiClient;
        private readonly ICarreraApiClient _carreraApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public EditModel(ICursoApiClient cursoApiClient, ICarreraApiClient carreraApiClient, ILoginApiClient loginApiClient)
        {
            _cursoApiClient = cursoApiClient;
            _carreraApiClient = carreraApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Curso CursoEditado { get; set; } = new();

        public List<Carrera>? Carreras { get; set; }

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

        public async Task<IActionResult> OnGetAsync(string id)
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

            Carreras = await _carreraApiClient.ObtenerTodasCarrerasAsync(token);
            var curso = await _cursoApiClient.ObtenerPorIdAsync(id, token);
            if (curso == null)
                return RedirectToPage("Index");

            CursoEditado = curso;
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

            Carreras = await _carreraApiClient.ObtenerTodasCarrerasAsync(token);

            if (!Regex.IsMatch(CursoEditado.Nombre ?? "", @"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò ]+$"))
            {
                ModelState.AddModelError("CursoEditado.Nombre", "El nombre solo puede contener letras y espacios.");
                return Page();
            }

            var (Exito, Mensaje, _) = await _cursoApiClient.CRUDCursoAsync(CursoEditado, "Update", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}

