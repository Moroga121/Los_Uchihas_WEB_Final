using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class DireccionesApiClient: IDireccionesApiClient
    {

        private readonly HttpClient _http;

        public DireccionesApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProvinciaDto>> ObtenerProvincias(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "provincias");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<List<ProvinciaDto>>(cancellationToken: ct);
        }

        public async Task<List<CantonDto>> ObtenerCantonesPorProvincia(string provincia, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"cantones?provincia={Uri.EscapeDataString(provincia)}");

            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {

                return new List<CantonDto>();

            }
                

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("cantones", out var cantonesJson))
            {

                return new List<CantonDto>();

            }
                

            return JsonSerializer.Deserialize<List<CantonDto>>(cantonesJson.GetRawText()) ?? new List<CantonDto>();

        }

        public async Task<List<DistritoDto>> ObtenerDistritosPorCantonProvincia(string provincia, string canton, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"distritos?provincia={Uri.EscapeDataString(provincia)}&canton={Uri.EscapeDataString(canton)}");

            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return new List<DistritoDto>();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("distritos", out var distritosJson))
                return new List<DistritoDto>();

            return JsonSerializer.Deserialize<List<DistritoDto>>(
                distritosJson.GetRawText()) ?? new List<DistritoDto>();
        }


    }
}
