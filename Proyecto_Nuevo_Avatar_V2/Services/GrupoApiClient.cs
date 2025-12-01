using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class GrupoApiClient : IGrupoApiClient
    {
        private readonly HttpClient _http;

        public GrupoApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Grupo>?> ObtenerTodosGruposAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/grupo/");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<Grupo>>();
        }

        public async Task<Grupo?> ObtenerGrupoPorIdAsync(string id, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/grupo/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Grupo>();
        }

        public async Task<(bool Exito, string Mensaje, Grupo? Datos)> CRUDGrupoAsync(Grupo grupo, string accion, string accessToken)
        {
            HttpMethod method;
            string endpoint = "api/grupo";
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
                    endpoint += $"/{grupo.ID_Grupo}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use Insert, Update o Delete.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);
            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(grupo);

            var response = await _http.SendAsync(request);
            var contenido = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Operación realizada correctamente.", grupo);

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

