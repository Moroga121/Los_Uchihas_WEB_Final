using Proyecto_Nuevo_Avatar_V2.Entities;
using static System.Net.WebRequestMethods;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class PrematriculaApiClient : IPrematriculaApiClient
    {
        private readonly HttpClient _http;
        public PrematriculaApiClient(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<Prematricula>?> Obtener_Todas_Prematriculas(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, "/Prematricula");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var prematriculas = await response.Content.ReadFromJsonAsync<List<Prematricula>>(cancellationToken: ct);
                return prematriculas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener roles: {ex.Message}");
                return null;
            }
        }
        public async Task<Prematricula?> ObtenerPrematriculaPorId(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Prematricula/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var prematricula = await response.Content.ReadFromJsonAsync<Prematricula>(cancellationToken: ct);
                return prematricula;
            }
            else
            {
                return null;
            }
        }

        #region CRUD Prematriculas
        public async Task<(bool Exito, string Mensaje, Prematricula? Datos)> CRUDPrematricula(Prematricula prematricula, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "/Prematricula";

            // Determinar método HTTP según la acción
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
                    endpoint += $"/{prematricula.id_prematricula}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(prematricula);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var prematriculacreado = System.Text.Json.JsonSerializer.Deserialize<Prematricula>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", prematriculacreado);
                }
                else
                {
                    // Si la API devuelve un mensaje de error tipo {"mensaje":"texto"}
                    var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
                    if (error != null && error.TryGetValue("mensaje", out var msg))
                        return (false, msg, null);

                    return (false, $"Error desconocido ({response.StatusCode})", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al procesar la respuesta: {ex.Message}", null);
            }
        }

        #endregion
    }
}
