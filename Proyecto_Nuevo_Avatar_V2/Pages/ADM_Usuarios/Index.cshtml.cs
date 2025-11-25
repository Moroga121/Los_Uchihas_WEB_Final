using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Usuarios
{
    public class IndexModel : PageModel
    {

        private readonly IUsuarioApiClient _usuarioApiClient;
        private readonly IRolesApiClient _rolesApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IUsuarioApiClient usuarioApiClient, IRolesApiClient rolesApiClient, ILoginApiClient loginApiClient, IConfiguration confi)
        {
            _usuarioApiClient = usuarioApiClient;
            _rolesApiClient = rolesApiClient;
            _loginApiClient = loginApiClient;
            _configuration = confi;
        }

        public List<UsuarioDto> Usuarios { get; set; } = new List<UsuarioDto>();

        public List<Rol> Roles { get; set; } = new List<Rol>();

        public List<Tipos_Identificacion> TiposIdentificacion { get; set; } = new List<Tipos_Identificacion>();

        public List<UsuarioDto> Dominios { get; set; } = new List<UsuarioDto>();

        #region "Paginación"

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        #endregion

        #region "Filtros de búsqueda"

        [BindProperty(SupportsGet = true)] public string FiltroIdentificacion { get; set; }
        [BindProperty(SupportsGet = true)] public string FiltroNombre { get; set; }
        [BindProperty(SupportsGet = true)] public string FiltroRol { get; set; }
        [BindProperty(SupportsGet = true)] public string FiltroTipo { get; set; }
        [BindProperty(SupportsGet = true)] public string FiltroDominio { get; set; }

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



        public async Task<IActionResult> OnGetAsync(int numeroPagina = 1)
        {

            var nombre = HttpContext.Session.GetString("Nombre");
            var email = HttpContext.Session.GetString("Email");
            var rol = HttpContext.Session.GetString("Rol");


            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(rol))
            {

                return RedirectToPage("/Login/Login");
            }


            ViewData["Nombre"] = nombre;
            ViewData["Email"] = email;
            ViewData["Rol"] = rol;

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            // Iniciar todas las llamadas API en paralelo

            var usuariosTask = _usuarioApiClient.ObtenerTodoslosUsuarios(token);
            var rolesTask = _rolesApiClient.ObtenerRolesAsync(token);
            var tiposIdentTask = _usuarioApiClient.ObtenerTiposIdentificacion(token);
            var dominiosTask = _usuarioApiClient.ObtenerDominios(token);

            // Esperar a que todas terminen

            await Task.WhenAll(usuariosTask, rolesTask, tiposIdentTask, dominiosTask);



            var usuarios = usuariosTask.Result ?? new List<UsuarioDto>();
            Roles = rolesTask.Result ?? new List<Rol>();
            TiposIdentificacion = tiposIdentTask.Result ?? new List<Tipos_Identificacion>();
            Dominios = dominiosTask.Result ?? new List<UsuarioDto>();

            bool hayFiltros = !string.IsNullOrEmpty(FiltroIdentificacion) || !string.IsNullOrEmpty(FiltroNombre) || !string.IsNullOrEmpty(FiltroRol) || !string.IsNullOrEmpty(FiltroTipo) || !string.IsNullOrEmpty(FiltroDominio);

            if (hayFiltros)
            {
                usuarios = await _usuarioApiClient.ObtenerUsuariosFiltradosAsync(
                    FiltroIdentificacion ?? "",
                    FiltroNombre ?? "",
                    FiltroRol ?? "",
                    FiltroTipo ?? "",
                    FiltroDominio ?? "",
                    token
                ) ?? new List<UsuarioDto>();


            }

            // Unir usuarios con nombre del rol

            Usuarios = usuarios.Select(u => new UsuarioDto
            {
                Identificacion = u.Identificacion,
                Tipo_Identificacion = u.Tipo_Identificacion,
                Nombre = u.Nombre,
                Email = u.Email,
                Contrasena = u.Contrasena,
                Rol_Usuario = u.Rol_Usuario,
                Nombre_Rol = Roles.FirstOrDefault(r => r.Identificador_Rol == u.Rol_Usuario)?.Nombre_Rol ?? "Sin rol"
            }).ToList();

            // Paginación

            TotalRegistros = Usuarios.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Usuarios = Usuarios.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            return Page();

        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            var usuarioEliminar = await _usuarioApiClient.ObtenerUsuarioPorIdentificacionAsync(id, token);
            if (usuarioEliminar == null)
            {
                TempData["Resultado"] = "El usuario no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _usuarioApiClient.CRUDUsuarios(usuarioEliminar, token, "Delete");

            TempData["Resultado"] = Exito ? "Usuario eliminado correctamente." : $"Error al eliminar el Usuario: {Mensaje}";
            TempData["TipoMensaje"] = Exito ? "exito" : "error";

            return RedirectToPage();
        }

    }
}
