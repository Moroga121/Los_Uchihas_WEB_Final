using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Roles
{
    public class CreateModel : PageModel
    {
        private readonly IRolesApiClient _rolesApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(IRolesApiClient rolesApiClient, ILoginApiClient loginApiClient)
        {
            _rolesApiClient = rolesApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Rol NuevoRol { get; set; } = new();

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
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var nuevoRol = new Rol
            {
                Identificador_Rol = NuevoRol.Identificador_Rol,
                Nombre_Rol = NuevoRol.Nombre_Rol,
            };

            var (Exito, Mensaje, Rol) = await _rolesApiClient.CRUDRoles(nuevoRol, token, "Insert");

            TempData["Resultado"] = Exito ? "Rol creado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();

        }
    }
}