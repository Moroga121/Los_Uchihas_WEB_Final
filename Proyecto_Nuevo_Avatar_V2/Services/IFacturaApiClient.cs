using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IFacturaApiClient
    {
        Task<List<FacturasDto>> ObtenerFacturasAsync(string accessToken, CancellationToken ct = default);
        Task<FacturasDto> ObtenerFacturaPorIdAsync(string accessToken, string id, CancellationToken ct = default);
        Task<FacturasDto> ReversarFacturaAsync(string accessToken, long numeroFactura, string motivo, CancellationToken ct = default);
        Task<List<DetalleFacturaDto>> ObtenerDetalleFacturaAsync(string accessToken, long idFactura, CancellationToken ct = default);
        Task<List<PeriodoDto>> ObtenerPeriodosAsync(string accessToken, CancellationToken ct = default);
        Task<List<MatriculaDto>> ObtenerMatriculasAsync(string accessToken, CancellationToken ct = default);
        Task<FacturasDto> CrearFacturaAsync(string accessToken, string identificacion, string periodo, int totalCursos, CancellationToken ct = default);
    }
}
