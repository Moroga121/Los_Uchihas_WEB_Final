using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ADM_Usuarios
{
    public class CreateModel : PageModel
    {


        private readonly IUsuarioApiClient _usuarioApiClient;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IRolesApiClient _rolesApiClient;

        public CreateModel(IUsuarioApiClient usuarioApiClient, ILoginApiClient loginApiClient, IRolesApiClient rolesApiClient)
        {

            _usuarioApiClient = usuarioApiClient;
            _loginApiClient = loginApiClient;
            _rolesApiClient = rolesApiClient;
            ListaRoles = new List<SelectListItem>();
            ListaTipos = new List<SelectListItem>();
        }

        [BindProperty]
        public UsuarioDto NuevoUsuario { get; set; } = new();

        public List<Rol>? Roles { get; set; }

        public List<Tipos_Identificacion>? TiposIdentificacion { get; set; }

        public List<SelectListItem> ListaTipos { get; set; }

        public List<SelectListItem> ListaRoles { get; set; }


        #region "Validar Token"
        private async Task<string?> GetValidAccessTokenAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

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
        private async Task CargarListasAsync(string token)
        {
            var rolesTask = _rolesApiClient.ObtenerRolesAsync(token);
            var tiposTask = _usuarioApiClient.ObtenerTiposIdentificacion(token);

            await Task.WhenAll(rolesTask, tiposTask);

            var roles = rolesTask.Result ?? new List<Rol>();
            var tipos = tiposTask.Result ?? new List<Tipos_Identificacion>();

            ListaRoles = roles.Select(r => new SelectListItem
            {
                Value = r.Identificador_Rol,
                Text = r.Nombre_Rol
            }).ToList();

            ListaTipos = tipos.Select(t => new SelectListItem
            {
                Value = t.ID_Identificacion,
                Text = t.Tipo_Identificacion
            }).ToList();
        }
        #endregion

        public async Task<IActionResult> OnGetAsync()
        {


            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                TempData["ErrorMessage"] = "Tu sesión ha expirado. Inicia sesión nuevamente.";
                return RedirectToPage("/Login/Login");
            }

            await CargarListasAsync(token);

            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                return RedirectToPage("/Login/Login");

            }

            await CargarListasAsync(token);

            if (!ModelState.IsValid)
            {

                return Page();
            }



            var nuevo_usu = new UsuarioDto
            {
                Identificacion = NuevoUsuario.Identificacion,
                Tipo_Identificacion = NuevoUsuario.Tipo_Identificacion,
                Nombre = NuevoUsuario.Nombre,
                Email = NuevoUsuario.Email,
                Contrasena = NuevoUsuario.Contrasena,
                Rol_Usuario = NuevoUsuario.Rol_Usuario,
            };

            var (Exito, Mensaje, Rol) = await _usuarioApiClient.CRUDUsuarios(nuevo_usu, token, "Insert");

            TempData["Resultado"] = Exito ? "Usuario creado correctamente." : Mensaje;
            TempData["TipoMensaje"] = Exito ? "exito" : "error";
            TempData["Redireccion"] = "Index";

            await CargarListasAsync(token);

            return Page();


        }

    }
}
