using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class InstitucionApiClient : IInstitucionApiClient
    {
        private readonly HttpClient _http;

        public InstitucionApiClient(HttpClient http)
        {
            _http = http;
        }

        #region Obtener / Buscar
        public async Task<List<Institucion>?> ObtenerInstitucionesAsync(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/institucion/");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<Institucion>>(cancellationToken: ct);
        }

        public async Task<List<Institucion>?> BuscarInstitucionesPorNombreAsync(string nombre, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/institucion/buscar?nombre={Uri.EscapeDataString(nombre)}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<Institucion>>(cancellationToken: ct);
        }

        public async Task<Institucion?> ObtenerInstitucionPorIdAsync(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/institucion/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Institucion>(cancellationToken: ct);
        }
        #endregion

        #region CRUD
        public async Task<(bool Exito, string Mensaje, Institucion? Datos)> CRUDInstitucionAsync(Institucion institucion, string accion, string accessToken, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "/api/institucion";

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
                    endpoint += $"/{institucion.ID_Institucion}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use Insert, Update o Delete.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(institucion);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Operación realizada con éxito.", institucion);
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
        #endregion
    }
}
