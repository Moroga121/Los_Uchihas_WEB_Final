using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class PeriodoApiClient : IPeriodoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PeriodoApiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Periodo>> ObtenerTodosAsync(string accessToken)
        {
            var baseUrl = _config["PeriodoApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/periodo");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<Periodo>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Periodo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Periodo>();
        }

        public async Task<Periodo?> ObtenerPorIdAsync(string id, string accessToken)
        {
            var baseUrl = _config["PeriodoApi:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/periodo/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Periodo>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<(bool Exito, string Mensaje, Periodo? Datos)> CRUDPeriodoAsync(Periodo periodo, string accion, string accessToken)
        {
            var baseUrl = _config["PeriodoApi:BaseUrl"];
            periodo.Accion = accion.Substring(0, 1).ToUpper();

            var json = JsonSerializer.Serialize(periodo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(
                accion.ToLower() switch
                {
                    "insert" => HttpMethod.Post,
                    "update" => HttpMethod.Put,
                    "delete" => HttpMethod.Delete,
                    _ => HttpMethod.Post
                },
                $"{baseUrl}/api/periodo{(accion == "Delete" ? "/" + periodo.ID_Periodo : "")}"
            );

            request.Headers.Add("access_token", accessToken);
            if (accion != "Delete") request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var jsonDoc = JsonDocument.Parse(result);
                var mensaje = jsonDoc.RootElement.GetProperty("mensaje").GetString() ?? "Operación completada.";
                return (response.IsSuccessStatusCode, mensaje, periodo);
            }
            catch
            {
                return (false, "Error al procesar la respuesta del servidor.", null);
            }
        }
    }
}

