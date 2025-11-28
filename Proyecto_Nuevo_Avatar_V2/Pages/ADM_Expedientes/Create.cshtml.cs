using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Expedientes
{
    public class CreateModel : PageModel
    {

        private readonly IExpedientesApiClient _expedientesApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(IExpedientesApiClient expedientesApiClient, ILoginApiClient loginApiClient)
        {
            _expedientesApiClient = expedientesApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public ExpedienteDto NuevoExpediente { get; set; } = new();

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
            var token = await GetValidAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            // Construir objeto a enviar a la API
            var expediente = new ExpedienteDto
            {
                numero_identificacion = NuevoExpediente.numero_identificacion,
                fecha_nacimiento = NuevoExpediente.fecha_nacimiento,
                id_distrito = NuevoExpediente.id_distrito,
                otras_senas = NuevoExpediente.otras_senas,
                telefono = NuevoExpediente.telefono
            };

            var (Exito, Mensaje, _) = await _expedientesApiClient.CRUDExpedientes(expediente, token, "Insert");

            TempData["Resultado"] = Exito ? "Expediente creado correctamente." : Mensaje;

            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }

    }
}
