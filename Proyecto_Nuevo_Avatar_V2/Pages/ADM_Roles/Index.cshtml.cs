using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using System.Reflection;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolesApiClient _rolApiClient;
        private readonly IModuloApiClient _moduloApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IRolesApiClient rolApiClient, IModuloApiClient moduloApiClient, ILoginApiClient loginApiClient, IConfiguration confi)
        {
            _rolApiClient = rolApiClient;
            _moduloApiClient = moduloApiClient;
            _loginApiClient = loginApiClient;
            _configuration = confi;
        }

        public List<Rol> Roles { get; set; } = new();
        public List<Modulo> Modulos { get; set; } = new();
        public List<Rol_Modulo> RolModulo { get; set; } = new();

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
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");

            var listaroles = await _rolApiClient.ObtenerRolesAsync(token);
            if (listaroles != null)
            {
                Roles = listaroles;
            }
            // Cargar módulos 

            var listamodulos = await _moduloApiClient.ObtenerModulosAsync(token);
            if (listamodulos != null)
            {
                Modulos = listamodulos;
            }

            //  permisos (Rol_Usuario actúa como relación)
            var listapermisos = await _moduloApiClient.ObtenerRol_UsuarioAsync(token);
            if (listapermisos != null)
            {
                RolModulo = listapermisos;
            }

            // Paginación

            TotalRegistros = Roles.Count;
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

            PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
            PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

            Roles = Roles.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();

            return Page();


        }
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }


            var rolAEliminar = await _rolApiClient.ObtenerRolPorId(id, token);
            if (rolAEliminar == null)
            {
                TempData["Resultado"] = "El rol no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _rolApiClient.CRUDRoles(rolAEliminar, token, "Delete");

            if (Exito)
            {
                TempData["Resultado"] = "Rol eliminado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = $"Error al eliminar el rol: {Mensaje}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActualizarPermisos(string rolId, List<string> modulosSeleccionados)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login/Login");
            }

            var (Exito, Mensaje, Datos) = await _rolApiClient.ActualizarPermisosAsync(
                rolId,
                modulosSeleccionados,
                token
            );
            if (!Exito)
            {
                TempData["Resultado"] = Mensaje;
                TempData["TipoMensaje"] = "error";
            }
            else
            {
                TempData["Resultado"] = "Permisos actualizados correctamente";
                TempData["TipoMensaje"] = "exito";
            }

            return RedirectToPage();
        }

    }
}
