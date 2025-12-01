using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class CarreraApiClient : ICarreraApiClient
    {
        private readonly HttpClient _http;

        public CarreraApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Carrera>?> ObtenerTodasCarrerasAsync(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/carrera/");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<Carrera>>(cancellationToken: ct);
        }

        public async Task<List<Carrera>?> ObtenerCarrerasPorInstitucionAsync(string idInstitucion, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/carrera/institucion/{idInstitucion}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<Carrera>>(cancellationToken: ct);
        }

        public async Task<Carrera?> ObtenerCarreraPorIdAsync(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/carrera/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Carrera>(cancellationToken: ct);
        }

        public async Task<(bool Exito, string Mensaje, Carrera? Datos)> CRUDCarreraAsync(Carrera carrera, string accion, string accessToken, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "api/carrera";
            switch (accion)
            {
                case "Insert":
                    method = HttpMethod.Post;
                    break;
                case "Update":
                    method = HttpMethod.Put;
                    break;
                case "Delete":
                    method = HttpMethod.Delete;
                    endpoint += $"/{carrera.ID_Carrera}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use Insert, Update o Delete.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(carrera);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Operación realizada con éxito.", carrera);
            }
            else
            {
                try
                {
                    var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
                    if (error != null && error.TryGetValue("mensaje", out var msg))
                        return (false, msg, null);
                }
                catch { }

                return (false, $"Error HTTP {response.StatusCode}", null);
            }
        }
    }
}


