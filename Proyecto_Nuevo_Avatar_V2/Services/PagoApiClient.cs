using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class PagoApiClient : IPagoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public PagoApiClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<Pago> ObtenerPagoPorNumeroAsync(string accessToken, int numeroPago, CancellationToken ct = default)
        {
            var url = $"/pago/{numeroPago}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new Exception(content);
            }

            var pago = await response.Content.ReadFromJsonAsync<Pago>(cancellationToken: ct);
            return pago;
        }

        public async Task<Pago> CrearPagoFacturaAsync(string accessToken, long numeroFactura, decimal montoPago, string rutaComprobante = null, CancellationToken ct = default)
        {
            var url = "/pago";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("access_token", accessToken);

            var body = new
            {
                numero_Factura = numeroFactura,
                monto = montoPago,
                ruta_Comprobante = rutaComprobante
            };

            var jsonContent = JsonContent.Create(body);
            request.Content = jsonContent;

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new Exception(content);
            }

            var pago = await response.Content.ReadFromJsonAsync<Pago>(cancellationToken: ct);
            return pago;
        }

        public async Task<List<Pago>> ObtenerPagoFacturasAsync(string accessToken, CancellationToken ct = default)
        {
            var url = $"/pago/a/";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return new List<Pago>();
            }

            var facturas = await response.Content.ReadFromJsonAsync<List<Pago>>(cancellationToken: ct);
            return facturas ?? new List<Pago>();
        }

        public async Task<decimal?> BuscarFacturaParaPagoAsync(string accessToken, long numeroFactura, CancellationToken ct = default)
        {
            var url = $"/pago/f/{numeroFactura}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");

                // Lanzar excepción para que el code-behind capture el mensaje de error del SP
                throw new Exception(content);
            }

            var resultado = await response.Content.ReadFromJsonAsync<FacturasDto>(cancellationToken: ct);
            return resultado?.MontoTotal;
        }

        public async Task<Pago> ReversarPagoAsync(string accessToken, int numeroPago, string motivo, CancellationToken ct = default)
        {
            var url = "/pago/";

            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Add("access_token", accessToken);

            var body = new
            {
                numero_Pago = numeroPago,
                motivo_Reversa = motivo
            };
            var jsonContent = JsonContent.Create(body);
            request.Content = jsonContent;

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return null;
            }

            var pago = await response.Content.ReadFromJsonAsync<Pago>(cancellationToken: ct);
            return pago;
        }

    }
}
