using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.Notificaciones
{
    public class IndexModel : PageModel
    {
        private readonly INotificacionApiClient _notificacionApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(INotificacionApiClient notificacionApiClient, ILoginApiClient loginApiClient)
        {
            _notificacionApiClient = notificacionApiClient;
            _loginApiClient = loginApiClient;
        }

        public List<Notificacion2Dto> Notificaciones { get; set; } = new();


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

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();

            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }
                

            var email = HttpContext.Session.GetString("Email");


            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login/Login");

            Notificaciones = await _notificacionApiClient.ObtenerNotificacionesAsync(token, email);

            return Page();
        }
    }
}
