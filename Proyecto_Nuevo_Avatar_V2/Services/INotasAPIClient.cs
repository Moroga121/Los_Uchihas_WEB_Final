using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface INotasAPIClient
    {
        Task<List<NotasDto>> ObtenerPromedio(string tipoIdentificacion, string numeroIdentificacion, string accessToken, int? año = null, string? periodo = null, CancellationToken ct = default);
        Task<List<string>> ObtenerPeriodosAsync(string accessToken, CancellationToken ct = default);
    }
}
