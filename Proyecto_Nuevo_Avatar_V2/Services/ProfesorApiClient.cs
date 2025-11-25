using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class ProfesorApiClient : IProfesorApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ProfesorApiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Profesor>> ObtenerProfesoresAsync(string accessToken)
        {
            var baseUrl = _config["ProfesorApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/profesor");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<Profesor>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Profesor>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Profesor>();
        }

        public async Task<Profesor?> ObtenerProfesorPorIdAsync(string id, string accessToken)
        {
            var baseUrl = _config["ProfesorApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/profesor/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Profesor>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<(bool Exito, string Mensaje, Profesor? Datos)> CRUDProfesorAsync(Profesor profesor, string accion, string accessToken)
        {
            var baseUrl = _config["ProfesorApi:BaseUrl"];
            profesor.Accion = accion.Substring(0, 1).ToUpper();

            var json = JsonSerializer.Serialize(profesor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(
                accion.ToLower() switch
                {
                    "insert" => HttpMethod.Post,
                    "update" => HttpMethod.Put,
                    "delete" => HttpMethod.Delete,
                    _ => HttpMethod.Post
                },
                $"{baseUrl}/api/profesor{(accion == "Delete" ? "/" + profesor.ID_Profesor : "")}"
            );

            request.Headers.Add("access_token", accessToken);
            if (accion != "Delete") request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(result))
                return (response.IsSuccessStatusCode,
                        response.IsSuccessStatusCode ? "Operación completada correctamente." : "Error: el servidor no devolvió respuesta.",
                        profesor);

            try
            {
                var jsonDoc = JsonDocument.Parse(result);
                bool exito = jsonDoc.RootElement.TryGetProperty("exito", out var exitoProp) && exitoProp.GetBoolean();
                string mensaje = jsonDoc.RootElement.TryGetProperty("mensaje", out var msgProp)
                    ? msgProp.GetString() ?? "Operación completada."
                    : "Operación completada.";

                return (exito, mensaje, profesor);
            }
            catch
            {
                return (response.IsSuccessStatusCode, result.Trim(), profesor);
            }
        }

        public async Task<IEnumerable<Profesor>> BuscarProfesoresAsync(
    string busqueda,
    string ordenCampo,
    string ordenDireccion,
    int pagina,
    int tamanoPagina,
    string token)
        {
            var baseUrl = _config["ProfesorApi:BaseUrl"];
            var url = $"{baseUrl}/api/profesor/buscar?" +
                      $"busqueda={Uri.EscapeDataString(busqueda ?? "")}" +
                      $"&ordenCampo={ordenCampo}" +
                      $"&ordenDireccion={ordenDireccion}" +
                      $"&pagina={pagina}" +
                      $"&tamanoPagina={tamanoPagina}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("access_token", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<Profesor>();

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Profesor>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<Profesor>();
            }
            catch
            {
                return new List<Profesor>();
            }
        }


    }
}


