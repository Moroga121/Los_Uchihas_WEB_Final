using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface ICarreraApiClient
    {
        Task<List<Carrera>?> ObtenerTodasCarrerasAsync(string accessToken, CancellationToken ct = default);
        Task<List<Carrera>?> ObtenerCarrerasPorInstitucionAsync(string idInstitucion, string accessToken, CancellationToken ct = default);
        Task<Carrera?> ObtenerCarreraPorIdAsync(string id, string accessToken, CancellationToken ct = default);
        Task<(bool Exito, string Mensaje, Carrera? Datos)> CRUDCarreraAsync(Carrera carrera, string accion, string accessToken, CancellationToken ct = default);
    }
}

