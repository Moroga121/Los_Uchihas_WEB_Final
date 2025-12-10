using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM17_Notas
{
    public class AsignarNotaModel : PageModel
    {
        private readonly IRubros_NotasApiClient _rubros_notasApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public AsignarNotaModel(IRubros_NotasApiClient notasApiClient, ILoginApiClient loginApiClient)
        {
            _rubros_notasApiClient = notasApiClient;
            _loginApiClient = loginApiClient;
        }
        [BindProperty] public string Curso { get; set; }
        [BindProperty] public string Grupo { get; set; }
        [BindProperty] public string Identificacion { get; set; }


        [BindProperty] public List<Rubros> Rubros { get; set; } = new();
        [BindProperty]public List<Notas> notas { get; set; } = new(); 

        public List<Notas> NotasOriginales { get; set; } = new(); 
        public bool TieneDesglose { get; set; } = false;
        public bool TieneNotas { get; set; } = false;
        [BindProperty(SupportsGet = true)]
        public bool ModoAgregar { get; set; }


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


        public async Task<IActionResult> OnGetAsync(string curso, string grupo, string identificacion, bool agregar = false)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            ModoAgregar = agregar;
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            // Obtener rubros
            var respuesta = await _rubros_notasApiClient.ObtenerDesglose(token, curso, grupo);

            // Obtener notas del estudiante
            var notasApi = await _rubros_notasApiClient.ObtenerNotas(token, curso, identificacion) ?? new List<Notas>(); ;

            // Validar desglose
            if (respuesta == null || respuesta.rubros == null || respuesta.rubros.Count == 0)
            {
                TieneDesglose = false;
                Curso = curso;
                Grupo = grupo;
                Mensaje = "Sin desglose.";
                return Page();
            }

            // Asignar datos generales
            TieneDesglose = true;
            Curso = respuesta.nombre_curso;
            Grupo = respuesta.nombre_grupo;
            Identificacion = identificacion;
            Rubros = respuesta.rubros;

            if (notasApi == null || notasApi.Count == 0)
            {
                TieneNotas = false;

                // Crear lista vacía de notas asociada a cada rubro
                notas = respuesta.rubros.Select(r => new Notas
                {
                    id_rubro = r.id_rubro,
                    numero_identificacion = identificacion,
                    valor = null
                }).ToList();

                // Si el usuario ya hizo clic en "Agregar notas", mostrar la tabla
                if (ModoAgregar)
                {
                    TieneNotas = true; 
                }

                Curso = respuesta.nombre_curso;
                Grupo = respuesta.nombre_grupo;
                Identificacion = identificacion;

                return Page();
            }

            TieneNotas = true;
            notas = notasApi;

            Mensaje = "Notas cargadas correctamente.";
            return Page();
        }

        public async Task<IActionResult> OnPostGuardar(string accion, int rowIndex)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            // Obtener solo una nota (la de la fila que se presionó)
            var notaSeleccionada = notas[rowIndex];

            var resultado = await _rubros_notasApiClient.AsignarNota(notaSeleccionada, accion, token);

            if (!resultado.Exito)
            {
                TempData["Resultado"] = resultado.Mensaje;
                TempData["TipoMensaje"] = "error";
                TempData["Redireccion"] = $"Index";
                return RedirectToPage(new { curso = Curso, grupo = Grupo, identificacion = Identificacion });
            }

            TempData["Resultado"] = "Operación realizada con éxito.";
            TempData["TipoMensaje"] = "exito";
            return RedirectToPage(new { curso = Curso, grupo = Grupo, identificacion = Identificacion });
        }



    }
}
