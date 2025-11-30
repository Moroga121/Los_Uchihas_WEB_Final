using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM17_Notas
{
    public class CargarDesgloseModel : PageModel
    {
        private readonly IRubros_NotasApiClient _rubros_notasApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public CargarDesgloseModel(IRubros_NotasApiClient notasApiClient, ILoginApiClient loginApiClient)
        {
            _rubros_notasApiClient = notasApiClient;
            _loginApiClient = loginApiClient;
        }
        [BindProperty]public string Curso { get; set; }
        [BindProperty] public string Grupo { get; set; }

        [BindProperty] public List<Rubros> Rubros { get; set; } = new();
        public bool TieneDesglose { get; set; } = false;
        public string Mensaje { get; set; } = "";

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


        public async Task<IActionResult> OnGetAsync(string curso, string grupo, bool nuevo = false)
        {
            if (nuevo)
            {
                TieneDesglose = true;
                Rubros = new List<Rubros>();  
                Curso = curso;
                Grupo = grupo;

                Mensaje = "Cargando nuevo desglose...";
                return Page();
            }

     
            // Obtener token
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            // Llamar API
            var respuesta = await _rubros_notasApiClient.ObtenerDesglose(token, curso, grupo);

            if (respuesta == null || respuesta.rubros == null || respuesta.rubros.Count == 0)
            {
                TieneDesglose = false;
                Curso = curso;
                Grupo = grupo;
                Mensaje = "Sin desglose.";
                return Page();
            }

            // CON DESGLOSE
            TieneDesglose = true;

            // Cargar propiedades del PageModel
            Curso = respuesta.nombre_curso;
            Grupo = respuesta.nombre_grupo;
            Rubros = respuesta.rubros;

            Mensaje = "Desglose cargado correctamente.";

            return Page();
        }
        public async Task<IActionResult> OnPostGuardarAsync()
        {
            // Validar token
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            // Construir objeto para enviar al API
            var desglose = new DesgloseRubro
            {
                nombre_curso = Curso,
                nombre_grupo = Grupo,
                rubros = Rubros   
            };

            // Llamar API
            var resultado = await _rubros_notasApiClient.CargarDesglose(desglose, token);

            if (!resultado.Exito)
            {
                TempData["Resultado"] = resultado.Exito ? "Operacion Fallida." : resultado.Mensaje;
                TempData["TipoMensaje"] = resultado.Exito ? "exito" : "error";
                TempData["Redireccion"] = "Index";
                return RedirectToPage(new { curso = Curso, grupo = Grupo });
            }

            TempData["Resultado"] = "Operacion Exitosa.";
            TempData["TipoMensaje"] = "exito";


            // Si querés recargar para ver los datos ingresados
            return RedirectToPage(new { curso = Curso, grupo = Grupo });
        }



    }
}
