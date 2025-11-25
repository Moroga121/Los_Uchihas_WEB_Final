using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IBitacoraApiClient
    {
        Task<List<BitacoraDto>> ObtenerBitacorasAsync(string accessToken, CancellationToken ct = default);
        Task<(bool, string mensaje)> RegistrarBitacoraAsync(string accion, string descripcion, string accessToken, CancellationToken ct = default);
        Task<List<BitacoraDto>> ObtenerBitacorasAsyncFiltradas(
                    string accessToken,
                    DateOnly? fechaInicio = null,
                    DateOnly? fechaFin = null,
                    string? usuario = null,
                    string? accion = null,
                    CancellationToken ct = default);
    }
}
