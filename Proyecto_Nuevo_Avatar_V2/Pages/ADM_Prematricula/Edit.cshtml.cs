using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Prematricula
{
    public class EditModel : PageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly ICursoApiClient _cursoApiClient;
        private readonly ICarreraApiClient _carreraApiClient;
        private readonly ILoginApiClient _loginApiClient;

        public EditModel(IPrematriculaApiClient prematriculaapiclient, ILoginApiClient loginApiClient, IPeriodoApiClient periodoApiClient, ICursoApiClient cursoApiClient, ICarreraApiClient carreraApiClient)
        {
            _prematriculaApiClient = prematriculaapiclient;
            _loginApiClient = loginApiClient;
            _periodoApiClient = periodoApiClient;
            _cursoApiClient = cursoApiClient;
            _carreraApiClient = carreraApiClient;
            _prematriculaApiClient = prematriculaapiclient;
        }

        [BindProperty]
        public Prematricula PrematriculaEditada { get; set; } = new();

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
        #region 
        public List<SelectListItem>? ListaPeriodos { get; set; }
        public List<SelectListItem>? ListaCarreras { get; set; }
        public List<SelectListItem>? ListaCursos { get; set; }
        #endregion
        private async Task CargarListasAsync(string token, Prematricula prematricula)
        {
            var periodosTask = _periodoApiClient.ObtenerTodosAsync(token);
            var carrerasTask = _carreraApiClient.ObtenerTodasCarrerasAsync(token);
            var cursosTask = _cursoApiClient.ObtenerTodosAsync(token);

            // Ejecutar todas las llamadas en paralelo
            await Task.WhenAll(periodosTask, carrerasTask, cursosTask);

            // Obtener resultados (si alguno da null, se reemplaza por lista vacía)
            var periodos = periodosTask.Result ?? new List<Periodo>();
            var carreras = carrerasTask.Result ?? new List<Carrera>();
            var cursos = cursosTask.Result ?? new List<Curso>();

            // Mapearlos a List<SelectListItem>
            ListaPeriodos = periodos.Select(p => new SelectListItem
            {
                Value = p.ID_Periodo.ToString(),
                Text = p.ID_Periodo,
                Selected = p.ID_Periodo == prematricula.Id_Periodo
            }).ToList();

            ListaCarreras = carreras.Select(c => new SelectListItem
            {
                Value = c.Nombre.ToString(),
                Text = c.Nombre,
                Selected = c.Nombre == prematricula.carrera
            }).ToList();

            ListaCursos = cursos.Select(c => new SelectListItem
            {
                Value = c.Nombre.ToString(),
                Text = c.Nombre,
                Selected = c.Nombre == prematricula.curso
            }).ToList();
        }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            // PRIMERO obtener la prematrícula
            var prematricula = await _prematriculaApiClient.ObtenerPrematriculaPorId(id, token);
            if (prematricula == null)
            {
                return RedirectToPage("Index");
            }

            PrematriculaEditada = prematricula;

            // AHORA sí cargar las listas con el valor correcto seleccionado
            await CargarListasAsync(token, PrematriculaEditada);

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

            // Ejecutar actualización
            var (Exito, Mensaje, PrematriculaActualizada) =
                await _prematriculaApiClient.CRUDPrematricula(PrematriculaEditada, token, "Update");

            TempData["Resultado"] = Exito ? "Prematricula actualizada correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            if (!Exito)
            {
                await CargarListasAsync(token, PrematriculaEditada);
                return Page(); 
            }

            // Si salió bien
            return RedirectToPage("Index");
        }

    }
}
