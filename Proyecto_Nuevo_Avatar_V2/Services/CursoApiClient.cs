using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class CursoApiClient : ICursoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public CursoApiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Curso>> ObtenerTodosAsync(string accessToken)
        {
            var baseUrl = _config["CursoApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}api/curso");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<Curso>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Curso>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Curso>();
        }

        public async Task<Curso?> ObtenerPorIdAsync(string id, string accessToken)
        {
            var baseUrl = _config["CursoApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}api/curso/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Curso>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<(bool Exito, string Mensaje, Curso? Datos)> CRUDCursoAsync(Curso curso, string accion, string accessToken)
        {
            var baseUrl = _config["CursoApi:BaseUrl"];
            curso.Accion = accion.Substring(0, 1).ToUpper();

            var json = JsonSerializer.Serialize(curso);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(
                accion.ToLower() switch
                {
                    "insert" => HttpMethod.Post,
                    "update" => HttpMethod.Put,
                    "delete" => HttpMethod.Delete,
                    _ => HttpMethod.Post
                },
                $"{baseUrl}api/curso{(accion == "Delete" ? "/" + curso.ID_Curso : "")}"
            );

            request.Headers.Add("access_token", accessToken);
            if (accion != "Delete") request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(result);
            var mensaje = jsonDoc.RootElement.GetProperty("mensaje").GetString() ?? "Operación completada.";

            return (response.IsSuccessStatusCode, mensaje, curso);
        }
    }
}

