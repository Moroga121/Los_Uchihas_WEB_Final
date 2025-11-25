using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IUsuarioApiClient
    {

        Task<UsuarioDto?> ObtenerUsuarioPorIdentificacionAsync(string id, string accessToken, CancellationToken ct = default);

        Task<UsuarioDto?> ObtenerUsuarioPorCorreoAsync(string id, string accessToken, CancellationToken ct = default);

        Task<List<UsuarioDto>> ObtenerTodoslosUsuarios(string accessToken, CancellationToken ct = default);

        Task<List<Tipos_Identificacion>> ObtenerTiposIdentificacion(string accessToken, CancellationToken ct = default);

        Task<(bool Exito, string Mensaje, UsuarioDto? Datos)> CRUDUsuarios(UsuarioDto usuarios, string accessToken, string accion, CancellationToken ct = default);

        Task<List<UsuarioDto>> ObtenerUsuariosFiltradosAsync(string identificacion, string nombre, string rol, string tipo, string dominio, string accessToken, CancellationToken ct = default);

        Task<List<UsuarioDto>> ObtenerDominios(string accessToken, CancellationToken ct = default);

        Task<(bool Exito, string Mensaje, UsuarioDto? Datos)> CambiarContrasenaAsync(UsuarioDto usuario, string accessToken, CancellationToken ct = default);

    }
}
