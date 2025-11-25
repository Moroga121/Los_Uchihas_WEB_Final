using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM9_Carreras
{
    public class EditModel : PageModel
    {
        private readonly ICarreraApiClient _carreraApiClient;
        private readonly IInstitucionApiClient _institucionApiClient;
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public EditModel(ICarreraApiClient carreraApiClient, IInstitucionApiClient institucionApiClient, IProfesorApiClient profesorApiClient, ILoginApiClient loginApiClient)
        {
            _carreraApiClient = carreraApiClient;
            _institucionApiClient = institucionApiClient;
            _profesorApiClient = profesorApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Carrera CarreraEditada { get; set; } = new();

        public List<Institucion>? Instituciones { get; set; }
        public List<Profesor>? Profesores { get; set; }

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

            Instituciones = await _institucionApiClient.ObtenerInstitucionesAsync(token);
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token);

            var carrera = await _carreraApiClient.ObtenerCarreraPorIdAsync(id, token);
            if (carrera == null)
                return RedirectToPage("Index");

            CarreraEditada = carrera;
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

            Instituciones = await _institucionApiClient.ObtenerInstitucionesAsync(token);
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token);

            if (!Regex.IsMatch(CarreraEditada.Nombre ?? "", @"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò ]+$"))
            {
                ModelState.AddModelError("CarreraEditada.Nombre", "El nombre solo puede contener letras y espacios.");
                return Page();
            }

            var (Exito, Mensaje, _) = await _carreraApiClient.CRUDCarreraAsync(CarreraEditada, "Update", token);

            TempData["Resultado"] = Exito ? "Carrera actualizada correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}


