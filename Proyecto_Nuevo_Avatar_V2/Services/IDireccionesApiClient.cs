using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IDireccionesApiClient
    {

        Task<List<ProvinciaDto>> ObtenerProvincias(string accessToken, CancellationToken ct = default);
        Task<List<CantonDto>> ObtenerCantonesPorProvincia(string provincia, string accessToken, CancellationToken ct = default);
        Task<List<DistritoDto>> ObtenerDistritosPorCantonProvincia(string provincia, string canton, string accessToken, CancellationToken ct = default);


    }
}
