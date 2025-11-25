using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IPagoApiClient
    {
        Task<List<Pago>> ObtenerPagoFacturasAsync(string accessToken, CancellationToken ct = default);
        Task<Pago> ReversarPagoAsync(string accessToken, int numeroPago, string motivo, CancellationToken ct = default);
        Task<decimal?> BuscarFacturaParaPagoAsync(string accessToken, long numeroFactura, CancellationToken ct = default);
        Task<Pago> CrearPagoFacturaAsync(string accessToken, long numeroFactura, decimal montoPago, string rutaComprobante = null, CancellationToken ct = default);
        Task<Pago> ObtenerPagoPorNumeroAsync(string accessToken, int numeroPago, CancellationToken ct = default);
    }
}
