using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Text.RegularExpressions;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM11_Profesores
{
    public class CreateModel : PageModel
    {
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(IProfesorApiClient profesorApiClient, ILoginApiClient loginApiClient)
        {
            _profesorApiClient = profesorApiClient;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        public Profesor NuevoProfesor { get; set; } = new();

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
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            NuevoProfesor.FechaNacimiento = DateTime.Today.AddYears(-25);
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

            if (!Regex.IsMatch(NuevoProfesor.Nombre ?? "", @"^[\p{L} ]+$"))
            {
                ModelState.AddModelError("NuevoProfesor.Nombre", "El nombre solo puede contener letras y espacios.");
                return Page();
            }

            var telefonoLimpio = Regex.Replace(NuevoProfesor.Telefono ?? "", @"\D", ""); 
            if (!Regex.IsMatch(telefonoLimpio, @"^[2-9][0-9]{7}$"))
            {
                ModelState.AddModelError("NuevoProfesor.Telefono", "El teléfono debe tener 8 dígitos válidos (####-#### o ########).");
                return Page();
            }
            NuevoProfesor.Telefono = telefonoLimpio.Insert(4, "-");

            var edad = DateTime.Today.Year - NuevoProfesor.FechaNacimiento.Year;
            if (NuevoProfesor.FechaNacimiento > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18)
            {
                ModelState.AddModelError("NuevoProfesor.FechaNacimiento", "El profesor debe ser mayor de edad.");
                return Page();
            }

            if (!NuevoProfesor.Email.EndsWith("@cuc.ac.cr", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("NuevoProfesor.Email", "El correo debe tener el dominio @cuc.ac.cr");
                return Page();
            }

            var (Exito, Mensaje, _) = await _profesorApiClient.CRUDProfesorAsync(NuevoProfesor, "Insert", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            return Page();
        }
    }
}



