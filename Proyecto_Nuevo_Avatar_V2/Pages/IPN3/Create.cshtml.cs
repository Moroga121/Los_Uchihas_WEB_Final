using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN3
{
    public class CreateModel : PageModel
    {
        private readonly INotificacionApiClient _notificacionApiClient;
        private readonly ILoginApiClient _loginApiClient;
        [BindProperty]
        public NotificacionDto Notificacion { get; set; } = new();

        public CreateModel(INotificacionApiClient notificacionApiClient, ILoginApiClient loginApiClient)
        {
            _notificacionApiClient = notificacionApiClient;
            _loginApiClient = loginApiClient;
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

        public void OnGet()
        {
        }

        public List<string> Logs { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var (Exito, Status, Mensaje) = await _notificacionApiClient.CreateAsync(Notificacion, token);

            if (Exito)
            {
                TempData["Resultado"] = "Notificación enviado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = Mensaje ?? "Error al enviar el notificación.";
                TempData["TipoMensaje"] = "error";
            }

            return Page();
        }

    }
}
