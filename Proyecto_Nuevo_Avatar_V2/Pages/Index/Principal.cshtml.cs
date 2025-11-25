using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.Index
{
    public class PrincipalModel : PageModel
    {
        private readonly ILoginApiClient _loginApiClient;
        private readonly IBitacoraApiClient _bitacoraApiClient;

        public PrincipalModel(ILoginApiClient loginApiClient, IBitacoraApiClient bitacoraApiClient)
        {
            _loginApiClient = loginApiClient;
            _bitacoraApiClient = bitacoraApiClient;
        }


        public async Task<IActionResult> OnGetAsync()
        {

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");


            var accessToken = HttpContext.Session.GetString("AccessToken");
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            // Registrar intento exitoso en la bitacora del login
            await _bitacoraApiClient.RegistrarBitacoraAsync(
               accion: "Visualizacion",
               descripcion: "Visualizacion del dashboard.",
               accessToken: accessToken
           );
            // Si no hay token redirige al login

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                TempData["ErrorMessage"] = "Tu sesión ha expirado. Inicia sesión nuevamente.";
                return RedirectToPage("/Login/Login");
            }

            // Validar token actual

            bool valido = await _loginApiClient.ValidateTokenAsync(accessToken);

            if (!valido)
            {
                // Renovar token
                var nuevoToken = await _loginApiClient.RefreshTokenAsync(refreshToken);

                if (nuevoToken != null)
                {
                    HttpContext.Session.SetString("AccessToken", nuevoToken.Access_Token);
                    HttpContext.Session.SetString("RefreshToken", nuevoToken.Refresh_Token);
                    HttpContext.Session.SetString("Expiresin", nuevoToken.Expires_In.ToString());

                }
                else
                {
                    // Si el refresh falla redirige a login

                    TempData["ErrorMessage"] = "Tu sesión ha expirado. Inicia sesión nuevamente.";
                    return RedirectToPage("/Login/Login");
                }
            }


            return Page();
        }

        public IActionResult OnPostLogout()
        {
            // Registrar intento exitoso en la bitacora del login
            _bitacoraApiClient.RegistrarBitacoraAsync(
              accion: "Cerrar sesion",
              descripcion: "Se cerro la sesion.",
              accessToken: HttpContext.Session.GetString("AccessToken")
          );
            HttpContext.Session.Clear();



            return RedirectToPage("/Login/Login");
        }

    }
}
