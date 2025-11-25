using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM8_Instituciones
{
    public class CreateModel : PageModel
    {
        private readonly IInstitucionApiClient _institucionApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(IInstitucionApiClient institucionApiClient, ILoginApiClient loginApiClient)
        {
            _institucionApiClient = institucionApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Institucion NuevaInstitucion { get; set; } = new();

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

        public void OnGet()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            if (!Regex.IsMatch(NuevaInstitucion.Nombre ?? "", @"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò ]+$"))
            {
                ModelState.AddModelError("NuevaInstitucion.Nombre", "El nombre solo puede contener letras y espacios.");
                return Page();
            }


            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var (Exito, Mensaje, _) = await _institucionApiClient.CRUDInstitucionAsync(NuevaInstitucion, "Insert", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}



