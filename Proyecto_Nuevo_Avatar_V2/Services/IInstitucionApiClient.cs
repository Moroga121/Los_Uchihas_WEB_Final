using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IInstitucionApiClient
    {
        Task<List<Institucion>?> ObtenerInstitucionesAsync(string accessToken, CancellationToken ct = default);
        Task<List<Institucion>?> BuscarInstitucionesPorNombreAsync(string nombre, string accessToken, CancellationToken ct = default);
        Task<Institucion?> ObtenerInstitucionPorIdAsync(string id, string accessToken, CancellationToken ct = default);
        Task<(bool Exito, string Mensaje, Institucion? Datos)> CRUDInstitucionAsync(Institucion institucion, string accion, string accessToken, CancellationToken ct = default);
    }
}
