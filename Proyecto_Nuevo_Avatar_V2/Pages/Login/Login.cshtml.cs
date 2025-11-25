using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.Login
{
    public class LoginModel : PageModel
    {

        private readonly ILoginApiClient _loginApiClient;
        private readonly IUsuarioApiClient _usuarioApiClient;

        public LoginModel(ILoginApiClient loginApiClient, IUsuarioApiClient usuarioApiClient)
        {
            _loginApiClient = loginApiClient;
            _usuarioApiClient = usuarioApiClient;
        }

        [BindProperty]
        public LoginDto Login { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {

                return Page();

            }
               

            try
            {
                var (ok, token, mensaje) = await _loginApiClient.LoginAsync(Login);

                if (!ok || token == null)
                {
                    ErrorMessage = mensaje ?? "Usuario o contraseña incorrectos.";
                    return Page();
                }

                // Guardar tokens en sesión

                HttpContext.Session.SetString("Expiresin", token.Expires_In.ToString());
                HttpContext.Session.SetString("AccessToken", token.Access_Token);
                HttpContext.Session.SetString("RefreshToken", token.Refresh_Token);
                HttpContext.Session.SetString("Email", token.Email);

                var usuario = await _usuarioApiClient.ObtenerUsuarioPorCorreoAsync(token.Email, token.Access_Token);

                if (usuario != null)
                {
                    HttpContext.Session.SetString("Nombre", usuario.Nombre);
                    HttpContext.Session.SetString("Rol", usuario.Rol_Usuario);
                }
                else
                {
                    ErrorMessage = "No se pudo obtener la información del usuario.";
                    return Page();
                }

                return RedirectToPage("/Index/Principal");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
                return Page();
            }
        }

        }
}
