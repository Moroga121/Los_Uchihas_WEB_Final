using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class BitacoraApiClient : IBitacoraApiClient
    {
        private readonly HttpClient _http;

        public BitacoraApiClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<(bool, string mensaje)> RegistrarBitacoraAsync(string accion, string descripcion, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "bitacora/registrar");

            // Agregar token al header
            request.Headers.Add("access_token", accessToken);

            // Crear el JSON a enviar
            var body = new
            {
                Accion = accion,
                Descripcion = descripcion
            };

            // Serializar a JSON
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.SendAsync(request, ct);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Bitácora registrada exitosamente");
                }
                else
                {
                    var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>(cancellationToken: ct);
                    if (payload != null && payload.TryGetValue("mensaje", out var m))
                    {
                        return (false, m ?? "Error desconocido al registrar bitácora");
                    }
                    return (false, "Error desconocido al registrar bitácora");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al procesar la respuesta de la API: {ex.Message}");
            }
        }

        public async Task<List<BitacoraDto>> ObtenerBitacorasAsync(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "bitacora/obtener_todas");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            // Leemos la lista original del endpoint
            var bitacoras = await response.Content.ReadFromJsonAsync<List<BitacoraDto>>(cancellationToken: ct)
                              ?? new List<BitacoraDto>();

            return bitacoras;
        }
        public async Task<List<BitacoraDto>> ObtenerBitacorasAsyncFiltradas(
            string accessToken,
            DateOnly? fechaInicio = null,
            DateOnly? fechaFin = null,
            string? usuario = null,
            string? accion = null,
            CancellationToken ct = default)
        {
            var queryParams = new List<string>();
            if (fechaInicio.HasValue)
                queryParams.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");
            if (fechaFin.HasValue)
                queryParams.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(usuario))
                queryParams.Add($"usuario={Uri.EscapeDataString(usuario)}");
            if (!string.IsNullOrWhiteSpace(accion))
                queryParams.Add($"accion={Uri.EscapeDataString(accion)}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;

            var request = new HttpRequestMessage(HttpMethod.Get, $"bitacora/obtener_todas-filtradas{queryString}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var bitacoras = await response.Content.ReadFromJsonAsync<List<BitacoraDto>>(cancellationToken: ct)
                              ?? new List<BitacoraDto>();

           return bitacoras;
        }



    }
}
