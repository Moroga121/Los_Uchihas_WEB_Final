using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Linq;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM9_Carreras
{
    public class IndexModel : PageModel
    {
        private readonly ICarreraApiClient _carreraApiClient;
        private readonly IInstitucionApiClient _institucionApiClient;
        private readonly IProfesorApiClient _profesorApiClient;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(
            ICarreraApiClient carreraApiClient,
            IInstitucionApiClient institucionApiClient,
            IProfesorApiClient profesorApiClient,
            IConfiguration configuration,
            ILoginApiClient loginApiClient)
        {
            _carreraApiClient = carreraApiClient;
            _institucionApiClient = institucionApiClient;
            _profesorApiClient = profesorApiClient;
            _configuration = configuration;
            _loginApiClient = loginApiClient;
        }

        public List<Carrera> Carreras { get; set; } = new(); 
        public List<Institucion> Instituciones { get; set; } = new(); 
        public List<Profesor> Profesores { get; set; } = new(); 

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

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

        #region "Paginación"

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        #endregion

        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
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

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            Carreras = await _carreraApiClient.ObtenerTodasCarrerasAsync(token) ?? new List<Carrera>();
            Instituciones = await _institucionApiClient.ObtenerInstitucionesAsync(token) ?? new List<Institucion>();
            Profesores = await _profesorApiClient.ObtenerProfesoresAsync(token) ?? new List<Profesor>();

            if (Carreras.Any())
            {
                foreach (var carrera in Carreras)
                {
                    var institucion = Instituciones.FirstOrDefault(i => i.ID_Institucion == carrera.ID_Institucion);
                    carrera.ID_Institucion = institucion != null ? institucion.Nombre : carrera.ID_Institucion;

                    var profesor = Profesores.FirstOrDefault(p => p.ID_Profesor == carrera.ID_Director);
                    carrera.ID_Director = profesor != null ? profesor.Nombre : carrera.ID_Director;
                }
            }

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                Busqueda = Busqueda.Trim();
                Carreras = Carreras
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.ID_Institucion) && c.ID_Institucion.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Nombre) && c.Nombre.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            TotalRegistros = Carreras.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Carreras = Carreras.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var carrera = new Carrera { ID_Carrera = id };
            var (Exito, Mensaje, _) = await _carreraApiClient.CRUDCarreraAsync(carrera, "Delete", token);

            TempData["Resultado"] = Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }
    }
}
