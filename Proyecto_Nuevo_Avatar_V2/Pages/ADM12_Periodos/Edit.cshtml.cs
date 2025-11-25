using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM12_Periodos
{
    public class EditModel : PageModel
    {
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public EditModel(IPeriodoApiClient periodoApiClient, ILoginApiClient loginApiClient)
        {
            _periodoApiClient = periodoApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Periodo PeriodoEditado { get; set; } = new();

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
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            var periodo = await _periodoApiClient.ObtenerPorIdAsync(id, token);
            if (periodo == null)
                return RedirectToPage("Index");

            PeriodoEditado = periodo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            if (PeriodoEditado.Año < 2000)
            {
                ModelState.AddModelError("PeriodoEditado.Año", "El año debe ser mayor o igual a 2000.");
                return Page();
            }

            if (PeriodoEditado.Numero_Periodo < 1 || PeriodoEditado.Numero_Periodo > 4)
            {
                ModelState.AddModelError("PeriodoEditado.Numero_Periodo", "El número de período debe estar entre 1 y 4.");
                return Page();
            }

            if (PeriodoEditado.Fecha_Fin < PeriodoEditado.Fecha_Inicio)
            {
                ModelState.AddModelError("PeriodoEditado.Fecha_Fin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                return Page();
            }

            if (PeriodoEditado.Fecha_Inicio.Year != PeriodoEditado.Año || PeriodoEditado.Fecha_Fin.Year != PeriodoEditado.Año)
            {
                ModelState.AddModelError("PeriodoEditado.Fecha_Inicio", "Las fechas deben pertenecer al mismo año indicado.");
                return Page();
            }

            var (Exito, Mensaje, _) = await _periodoApiClient.CRUDPeriodoAsync(PeriodoEditado, "Update", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return RedirectToPage();
        }
    }
}
