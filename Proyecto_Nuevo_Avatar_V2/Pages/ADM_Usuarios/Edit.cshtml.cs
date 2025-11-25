using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Usuarios
{
    public class EditModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IRolesApiClient _rolesApiClient;


        public EditModel(IUsuarioApiClient usuarioApiClient, ILoginApiClient loginApiClient, IRolesApiClient rolesApiClient)
        {
            _usuarioApiClient = usuarioApiClient;
            _loginApiClient = loginApiClient;
            _rolesApiClient = rolesApiClient;
            ListaRoles = new List<SelectListItem>();
            ListaTipos = new List<SelectListItem>();

        }

        [BindProperty]
        public UsuarioDto UsuarioActualizado { get; set; } = new();

        public List<Rol>? Roles { get; set; }

        public List<SelectListItem> ListaRoles { get; set; }

        public List<SelectListItem> ListaTipos { get; set; }


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

        #region Cargar listas
        private async Task CargarListasAsync(string token, UsuarioDto usuario)
        {
            var rolesTask = _rolesApiClient.ObtenerRolesAsync(token);
            var tiposTask = _usuarioApiClient.ObtenerTiposIdentificacion(token);

            await Task.WhenAll(rolesTask, tiposTask);

            var roles = rolesTask.Result ?? new List<Rol>();
            var tipos = tiposTask.Result ?? new List<Tipos_Identificacion>();

            ListaRoles = roles.Select(r => new SelectListItem
            {
                Value = r.Identificador_Rol,
                Text = r.Nombre_Rol,
                Selected = r.Identificador_Rol == usuario.Rol_Usuario
            }).ToList();

            ListaTipos = tipos.Select(t => new SelectListItem
            {
                Value = t.ID_Identificacion,
                Text = t.Tipo_Identificacion,
                Selected = t.ID_Identificacion == usuario.Tipo_Identificacion
            }).ToList();


            

        }
        #endregion


        #region "Normalizar Listas"

        private async Task NormalizarRolUsuarioAsync(string token, UsuarioDto usuario)
        {
            var roles = await _rolesApiClient.ObtenerRolesAsync(token);

            
            var rol = roles.FirstOrDefault(r =>
                r.Nombre_Rol.Equals(usuario.Rol_Usuario, StringComparison.OrdinalIgnoreCase));

            if (rol != null)
            {
                
                usuario.Rol_Usuario = rol.Identificador_Rol;
            }
        }


        private async Task NormalizarTipoIdentificacionAsync(string token, UsuarioDto usuario)
        {
            var tipos = await _usuarioApiClient.ObtenerTiposIdentificacion(token);

            
            var tipo = tipos.FirstOrDefault(t =>
                t.Tipo_Identificacion.Equals(usuario.Tipo_Identificacion, StringComparison.OrdinalIgnoreCase));

            if (tipo != null)
            {
                
                usuario.Tipo_Identificacion = tipo.ID_Identificacion;
            }
        }

        #endregion

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");


            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            // Obtener el usuario por ID
            var usuario = await _usuarioApiClient.ObtenerUsuarioPorIdentificacionAsync(id, token);
            if (usuario == null)
                return RedirectToPage("Index");

            UsuarioActualizado = usuario;

            await NormalizarRolUsuarioAsync(token, UsuarioActualizado);

            await NormalizarTipoIdentificacionAsync(token, UsuarioActualizado);

            await CargarListasAsync(token, UsuarioActualizado);

            return Page();

        }



        public async Task<IActionResult> OnPostAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
                return RedirectToPage("/Login/Login");

            await CargarListasAsync(token, UsuarioActualizado);

            if (!ModelState.IsValid)
            {

                return Page();
            }

            // Actualizar usuario
            var (Exito, Mensaje, _) = await _usuarioApiClient.CRUDUsuarios(UsuarioActualizado, token, "Update");

            TempData["Resultado"] = Exito ? "Usuario actualizado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            await CargarListasAsync(token, UsuarioActualizado);

            return Page();

        }
    }
}
