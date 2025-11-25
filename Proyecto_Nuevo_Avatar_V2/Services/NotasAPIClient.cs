using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class NotasAPIClient : INotasAPIClient
    { 
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public NotasAPIClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<List<NotasDto>> ObtenerPromedio(string tipoIdentificacion, string numeroIdentificacion, string accessToken, int? año = null, string? periodo = null, CancellationToken ct = default)
        {
            try
            {
                var url = $"/historialacademico/{tipoIdentificacion}/{numeroIdentificacion}";

                var queryParams = new List<string>();

                if (año.HasValue)
                    queryParams.Add($"año={año.Value}");

                if (!string.IsNullOrEmpty(periodo))
                    queryParams.Add($"periodo={periodo}");

                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<NotasDto>();
                }

                var notificaciones = await response.Content.ReadFromJsonAsync<List<NotasDto>>(cancellationToken: ct);
                return notificaciones ?? new List<NotasDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener notificaciones: {ex.Message}");
                return new List<NotasDto>();
            }
        }

        public async Task<List<string>> ObtenerPeriodosAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                var periodoApiUrl = _configuration["PeriodoApi:BaseUrl"];

                if (string.IsNullOrEmpty(periodoApiUrl))
                {
                    Console.WriteLine("PeriodoApi:BaseUrl no está configurado");
                    return new List<string>();
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
                    return new List<string>();
                }

                var periodos = await response.Content.ReadFromJsonAsync<List<PeriodoDto>>(cancellationToken: ct);

                var periodosUnicos = periodos?
                    .Select(p => p.ID_Periodo)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList() ?? new List<string>();

                return periodosUnicos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener periodos: {ex.Message}");
                return new List<string>();
            }
        }

    }
}

