using System.Net;
using System.Text.Json;
using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class NotificacionApiClient : INotificacionApiClient
    {
        private readonly HttpClient _http;
        

        public NotificacionApiClient(HttpClient http)
        {
            _http = http;
            
        }


        public async Task<List<Notificacion2Dto>> ObtenerNotificacionesAsync(string accessToken, string email, CancellationToken ct = default)
        {
            // Construye la URL con el email en la ruta
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:8004/notificar/{email}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var notificaciones = await response.Content.ReadFromJsonAsync<List<Notificacion2Dto>>(cancellationToken: ct);
            return notificaciones ?? new List<Notificacion2Dto>();
        }

        public async Task<List<NotificacionHistorialDto>> ObtenerNotificaciones(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request
                var request = new HttpRequestMessage(HttpMethod.Get, "/notificar");
                request.Headers.Add("access_token", accessToken);

                // Enviar la petición
                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<NotificacionHistorialDto>();
                }

                // Leer el contenido JSON
                var notificaciones = await response.Content.ReadFromJsonAsync<List<NotificacionHistorialDto>>(cancellationToken: ct);
                return notificaciones ?? new List<NotificacionHistorialDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener notificaciones: {ex.Message}");
                return new List<NotificacionHistorialDto>();
            }
        }

        public async Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(NotificacionDto notificacion, string accessToken, CancellationToken ct = default)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(notificacion, options);
                Console.WriteLine($"JSON ENVIADO: {json}");

                var request = new HttpRequestMessage(HttpMethod.Post, "notificar")
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };

                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);
                var responseContent = await response.Content.ReadAsStringAsync(ct);

                Console.WriteLine($"STATUS CODE: {response.StatusCode}");
                Console.WriteLine($"RESPUESTA DEL API: {responseContent}");

                string? msg = null;
                if (!string.IsNullOrEmpty(responseContent))
                {
                    var payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(responseContent);
                    if (payload != null && payload.TryGetValue("mensaje", out var m))
                    {
                        msg = m?.ToString();
                    }
                }

                return (response.StatusCode == System.Net.HttpStatusCode.Created, response.StatusCode, msg);
            }
            catch (Exception ex)
            {
                return (false, System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
