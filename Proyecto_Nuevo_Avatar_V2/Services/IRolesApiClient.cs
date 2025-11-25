using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IRolesApiClient
    {
        Task<(bool Exito, string Mensaje, Rol? Datos)> CRUDRoles(Rol roles, string accessToken, string accion, CancellationToken ct = default);
        Task<List<Rol>?> ObtenerRolesAsync(string accessToken, CancellationToken ct = default);
        Task<Rol?> ObtenerRolPorId(string id, string accessToken, CancellationToken ct = default);

        Task<(bool Exito, string Mensaje, Rol_Modulo Datos)> ActualizarPermisosAsync(string rolid, List<string> modulos, string accessToken, CancellationToken ct = default);
    }
}
