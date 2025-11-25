using Microsoft.AspNetCore.Mvc;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Components
{
    public class MenuLateralViewComponent : ViewComponent
    {
        private readonly IModuloApiClient _menuService;

        public MenuLateralViewComponent(IModuloApiClient menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var modulos = new List<Modulo>();

            var accessToken = HttpContext.Session.GetString("AccessToken");
            var rol = HttpContext.Session.GetString("Rol");

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(rol))
            {
                modulos = await _menuService.ObtenerMenuPorRolAsync(rol, accessToken)
                    ?? new List<Modulo>();
            }

            //Evita NullReference en la vista
            foreach (var m in modulos)
                m.Opciones ??= new List<Opcion>();

            return View("~/Pages/Shared/Components/MenuLateral/Default.cshtml", modulos);
        }
    }
}
