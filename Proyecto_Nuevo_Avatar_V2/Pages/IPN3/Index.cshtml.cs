using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN3
{
    public class IndexModel : PageModel
    {
            private readonly INotificacionApiClient _notificacionApiClient;
            private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;
        public IndexModel(INotificacionApiClient notificacionApiClient, IConfiguration confi, ILoginApiClient loginApiClient)
            {
            _notificacionApiClient = notificacionApiClient;
            _configuration = confi;
            _loginApiClient = loginApiClient;
        }

        public List<NotificacionDto> Notificacion { get; set; } = new();

        public List<NotificacionHistorialDto> Notificaciones { get; set; } = new();


        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                Notificaciones = new List<NotificacionHistorialDto>(); 
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }
            
            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            Notificaciones = await _notificacionApiClient.ObtenerNotificaciones(token);

            TotalRegistros = Notificaciones.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Notificaciones = Notificaciones.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            return Page();
        }

    }

}


