using Proyecto_Nuevo_Avatar_V2.Entities;
using System;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class Rubros_NotasApiClient : IRubros_NotasApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public Rubros_NotasApiClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        #region "Desglose "
        public async Task<(bool Exito, string Mensaje,DesgloseRubro?)> CargarDesglose(DesgloseRubro desglose,string accessToken,CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "cargardesglose");
            request.Headers.Add("access_token", accessToken);  
            request.Content = JsonContent.Create(desglose);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var desglosecargado = System.Text.Json.JsonSerializer.Deserialize<DesgloseRubro>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", desglosecargado);
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
        public async Task<DesgloseRubro?> ObtenerDesglose(string accessToken,string curso,string grupo, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, $"obtenerdesglose/{curso}/{grupo}");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var desglose = await response.Content.ReadFromJsonAsync<DesgloseResponse>(cancellationToken: ct);
                return desglose?.data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener desglose: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Notas
        public async Task<(bool Exito, string Mensaje, Notas?)> AsignarNota(Notas nota,string accion, string accessToken, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "asignarnotarubro";

            // Determinar método HTTP según la acción
            switch (accion)
            {
                case "Insert":
                    method = HttpMethod.Post;
                    break;
                case "Update":
                    method = HttpMethod.Put;
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }
            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(nota);


            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var notaprocesada = System.Text.Json.JsonSerializer.Deserialize<Notas>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", notaprocesada);
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
        public async Task<List<Notas>?> ObtenerNotas(string accessToken, string curso, string identificacion, CancellationToken ct = default)
        {
            try
            {
                var url = $"obtenernotas/{Uri.EscapeDataString(identificacion)}/{Uri.EscapeDataString(curso)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }

                var notas = await response.Content.ReadFromJsonAsync<NotasResponse>(cancellationToken: ct);

                return notas?.data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener notas: {ex.Message}");
                return null;
            }
        }


        #endregion
    }
}
