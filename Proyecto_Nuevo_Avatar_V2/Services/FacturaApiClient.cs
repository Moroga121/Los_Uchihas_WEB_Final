using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class FacturaApiClient : IFacturaApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public FacturaApiClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<List<DetalleFacturaDto>> ObtenerDetalleFacturaAsync(string accessToken, long idFactura, CancellationToken ct = default)
        {
            var url = $"/factura/d?idFactura={idFactura}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return new List<DetalleFacturaDto>();
            }

            var detalles = await response.Content.ReadFromJsonAsync<List<DetalleFacturaDto>>(cancellationToken: ct);
            return detalles ?? new List<DetalleFacturaDto>();
        }
        public async Task<List<FacturasDto>> ObtenerFacturasAsync(string accessToken, CancellationToken ct = default)
        {
            var url = $"/factura";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return new List<FacturasDto>();
            }

            var facturas = await response.Content.ReadFromJsonAsync<List<FacturasDto>>(cancellationToken: ct);
            return facturas ?? new List<FacturasDto>();
        }

        public async Task<FacturasDto> ObtenerFacturaPorIdAsync(string accessToken, string id, CancellationToken ct = default)
        {
            var url = $"/factura/{id}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return null;
            }

            var factura = await response.Content.ReadFromJsonAsync<FacturasDto>(cancellationToken: ct);
            return factura;
        }

        public async Task<FacturasDto> ReversarFacturaAsync(string accessToken, long numeroFactura, string motivo, CancellationToken ct = default)
        {
            var url = "/factura/";

            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Add("access_token", accessToken);

            // Crear el objeto para el body
            var body = new
            {
                numeroFactura = numeroFactura,
                motivo = motivo
            };

            // Serializar a JSON y agregarlo al contenido de la request
            var jsonContent = JsonContent.Create(body);
            request.Content = jsonContent;

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                return null;
            }

            var factura = await response.Content.ReadFromJsonAsync<FacturasDto>(cancellationToken: ct);
            return factura;
        }

        public async Task<List<PeriodoDto>> ObtenerPeriodosAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                var periodoApiUrl = _configuration["PeriodoApi:BaseUrl"];

                if (string.IsNullOrEmpty(periodoApiUrl))
                {
                    Console.WriteLine("PeriodoApi:BaseUrl no está configurado");
                    return new List<PeriodoDto>();
                }

                using var client = new HttpClient();
                client.BaseAddress = new Uri(periodoApiUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/periodo");
                request.Headers.Add("access_token", accessToken);

                var response = await client.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error al obtener periodos HTTP {(int)response.StatusCode}: {content}");
                    return new List<PeriodoDto>();
                }

                var periodos = await response.Content.ReadFromJsonAsync<List<PeriodoDto>>(cancellationToken: ct);
                return periodos ?? new List<PeriodoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener periodos: {ex.Message}");
                return new List<PeriodoDto>();
            }
        }
        public async Task<List<MatriculaDto>> ObtenerMatriculasAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                var matriculaApiUrl = _configuration["ListadoApi:BaseUrl"];

                if (string.IsNullOrEmpty(matriculaApiUrl))
                {
                    Console.WriteLine("ListadoApi:BaseUrl no está configurado");
                    return new List<MatriculaDto>();
                }

                using var client = new HttpClient();
                client.BaseAddress = new Uri(matriculaApiUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, "/matriculas");
                request.Headers.Add("access_token", accessToken);

                var response = await client.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error al obtener matrículas HTTP {(int)response.StatusCode}: {content}");
                    return new List<MatriculaDto>();
                }

                var matriculas = await response.Content.ReadFromJsonAsync<List<MatriculaDto>>(cancellationToken: ct);
                return matriculas ?? new List<MatriculaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener matrículas: {ex.Message}");
                return new List<MatriculaDto>();
            }
        }

        public async Task<FacturasDto> CrearFacturaAsync(string accessToken, string identificacion, string periodo, int totalCursos, CancellationToken ct = default)
        {
            try
            {
                var url = "/factura";

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("access_token", accessToken);

                const int precioPorCurso = 30000;
                var montoBase = totalCursos * precioPorCurso;

                // Crear el body
                var body = new
                {
                    identificacion = identificacion,
                    montoBase = montoBase,
                    periodo = periodo
                };

                var jsonContent = JsonContent.Create(body);
                request.Content = jsonContent;

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    return null;
                }

                var factura = await response.Content.ReadFromJsonAsync<FacturasDto>(cancellationToken: ct);
                return factura;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}

