using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Parametros
{
    public class EditModel : PageModel
    {
        private readonly IParametrosApiClient _parametroApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public EditModel(IParametrosApiClient parametroApiClient, ILoginApiClient loginApiClient)
        {
            _parametroApiClient = parametroApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Parametrizacion ParametroEditado { get; set; } = new();
        #region 
        public List<SelectListItem>? ListaPeriodos { get; set; }
        public List<SelectListItem>? ListaCarreras { get; set; }
        public List<SelectListItem>? ListaCursos { get; set; }
        #endregion

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
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            var parametro = await _parametroApiClient.ObtenerParametrolPorId(id, token);
            if (parametro == null)
            {

                return RedirectToPage("Index");

            }


            ParametroEditado = parametro;
            return Page();
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

            var (Exito, Mensaje, RolActualizado) = await _parametroApiClient.CRUDParametros(ParametroEditado, token, "Update");

            TempData["Resultado"] = Exito ? "Parámetro actualizado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}
