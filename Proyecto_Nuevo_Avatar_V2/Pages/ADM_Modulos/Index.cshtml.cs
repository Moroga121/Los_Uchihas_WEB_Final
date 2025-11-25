using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Modulos
{
    public class IndexModel : PageModel
    {
        private readonly IModuloApiClient _moduloApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IRolesApiClient _rolApiClient;

        public IndexModel(IModuloApiClient moduloService, ILoginApiClient loginApiClient, IRolesApiClient rolApiClient)
        {
            _moduloApiClient = moduloService;
            _loginApiClient = loginApiClient;
            _rolApiClient = rolApiClient;
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

        // Cargar todos los módulos
        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }


            var listaroles = await _rolApiClient.ObtenerRolesAsync(token);
            if (listaroles != null)
            {
                Roles = listaroles;
            }
            // Cargar módulos (en la práctica desde la BD)

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


            return Page();

        }

        // Eliminar módulo
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var moduloeliminar = await _moduloApiClient.ObtenerModuloPorId(id, token);
            if (moduloeliminar == null)
            {
                TempData["Resultado"] = "El modulo no existe.";
                TempData["TipoMensaje"] = "error";
                return RedirectToPage();
            }

            var (Exito, Mensaje, _) = await _moduloApiClient.CRUDModulos(moduloeliminar, token, "Delete");

            if (Exito)
            {
                TempData["Resultado"] = "Modulo eliminado correctamente.";
                TempData["TipoMensaje"] = "exito";
            }
            else
            {
                TempData["Resultado"] = $"Error al eliminar el rol: {Mensaje}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostActualizarPermisos(string moduloId, List<string> rolesSeleccionados)
        {

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var (Exito, Mensaje, Datos) = await _moduloApiClient.ActualizarPermisosModuloAsync(
                moduloId,
                rolesSeleccionados,
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
