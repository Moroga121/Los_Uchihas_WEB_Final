using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Usuarios
{
    public class CambiarContrasenaModel : PageModel
    {


        private readonly ILoginApiClient _loginApiClient;
        private readonly IUsuarioApiClient _usuarioApiClient;
        private readonly ContrasenaSettings _settings;

        public CambiarContrasenaModel(IUsuarioApiClient usuarioApiClient, ILoginApiClient loginApiClient, IOptions<ContrasenaSettings> policyOptions)
        {
            _usuarioApiClient = usuarioApiClient;
            _loginApiClient = loginApiClient;
            _settings = policyOptions.Value;
        }

        [BindProperty]
        public UsuarioDto Usuario { get; set; } = new();


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

        #region "Validar Contraseña"

        private string? ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                return "La contraseña no puede estar vacía.";

            if (contrasena.Length < _settings.MinLength || contrasena.Length > _settings.MaxLength)
            {

                return $"Debe tener entre {_settings.MinLength} y {_settings.MaxLength} caracteres.";

            }


            if (_settings.RequireUppercase && !Regex.IsMatch(contrasena, "[A-Z]"))
            {

                return "Debe incluir al menos una letra mayúscula.";

            }


            if (_settings.RequireLowercase && !Regex.IsMatch(contrasena, "[a-z]"))
            {

                return "Debe incluir al menos una letra minúscula.";

            }

            if (_settings.RequireDigit && !Regex.IsMatch(contrasena, "[0-9]"))
            {

                return "Debe incluir al menos un número.";

            }


            if (_settings.RequireSpecialCharacter && !Regex.IsMatch(contrasena, @"[\W_]"))
            {

                return "Debe incluir al menos un carácter especial.";

            }

            return null;
        }

        #endregion

        public void OnGet()
        {
            Usuario.Nombre = HttpContext.Session.GetString("Nombre") ?? "";
            Usuario.Email = HttpContext.Session.GetString("Email") ?? "";
            Usuario.Rol_Usuario = HttpContext.Session.GetString("Rol") ?? "";

            ViewData["Nombre"] = Usuario.Nombre;
            ViewData["Email"] = Usuario.Email;
            ViewData["Rol"] = Usuario.Rol_Usuario;

        }


        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }


            ViewData["Nombre"] = Usuario.Nombre;
            ViewData["Email"] = Usuario.Email;
            ViewData["Rol"] = Usuario.Rol_Usuario;


            Usuario.Email = HttpContext.Session.GetString("Email") ?? "";

            var error = ValidarContrasena(Usuario.Contrasena);
            if (error != null)
            {
                TempData["Resultado"] = error;
                TempData["TipoMensaje"] = "error";
                return Page();
            }


            var (Exito, Mensaje, Datos) = await _usuarioApiClient.CambiarContrasenaAsync(Usuario, token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Login";

            if (Exito && Datos != null)
            {

                HttpContext.Session.SetString("Email", Datos.Email);
                HttpContext.Session.SetString("Contrasena", Datos.Contrasena);

                HttpContext.Session.Clear();

            }

            return Page();
        }


    }
}
