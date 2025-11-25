using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IListadoPeriodoApiClient
    {
        Task<List<ListadoPeriodoDto>> ObtenerMatriculadosAsync(string accessToken, string periodo, CancellationToken ct = default);
        Task<List<string>> ObtenerPeriodosAsync(string accessToken, CancellationToken ct = default);
    }
}
