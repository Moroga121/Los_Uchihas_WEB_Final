using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Matricula
{
    public class CreateModel : PageModel
    {
        private readonly IMatriculaApiClient _matriculaApiClient;
        private readonly IPrematriculaApiClient _prematriculaApiClient;
        private readonly IGrupoApiClient _grupoApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CreateModel(IMatriculaApiClient matriculaApiClient, IPrematriculaApiClient prematriculaApiClient, ILoginApiClient loginApiClient, IGrupoApiClient grupoApiClient)
        {
            _matriculaApiClient = matriculaApiClient;
            _prematriculaApiClient = prematriculaApiClient;
            _loginApiClient = loginApiClient;
            _grupoApiClient = grupoApiClient;
        }

        [BindProperty]
        public MatriculaDto Matricula { get; set; } = new();

        public List<Grupo> Grupos { get; set; } = new();

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

        public async Task<IActionResult> OnGetAsync(int idPrematricula)
        {
            var token = await GetValidAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {

                return RedirectToPage("/Login/Login");

            }
                

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            Grupos = await _grupoApiClient.ObtenerTodosGruposAsync(token) ?? new();


            // Obtener datos de la prematricula

            var prem = await _prematriculaApiClient.ObtenerPrematriculaPorId(idPrematricula.ToString(), token);
            if (prem == null)
            {

                return RedirectToPage("Index");

            }
                

            // Copiar los datos relevantes

            Matricula.Numero_Identificacion = prem.numero_identificacion;
            Matricula.Curso = prem.curso;
            Matricula.Id_Periodo = prem.Id_Periodo;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {

                return RedirectToPage("/Login/Login");

            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            Grupos = await _grupoApiClient.ObtenerTodosGruposAsync(token) ?? new();

            if (!ModelState.IsValid)
            {

                return Page();

            }
                

            var (Exito, Mensaje, _) = await _matriculaApiClient.CRUDMatricula(Matricula, token, "Crear");

            TempData["Resultado"] = Exito ? "Matrícula creada correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "/ADM_Matricula/Index";

            return Page();
        }
    }
}
