using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class ExpedienteApiClient: IExpedientesApiClient
    {


        private readonly HttpClient _http;
        public ExpedienteApiClient(HttpClient http)
        {
            _http = http;
        }


        public async Task<List<ExpedienteDto>?> Obtener_Todos_Expedientes(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, "expediente");
                request.Headers.Add("access_token", accessToken); 

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var expedinte = await response.Content.ReadFromJsonAsync<List<ExpedienteDto>>(cancellationToken: ct);
                return expedinte;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener roles: {ex.Message}");
                return null;
            }
        }
        public async Task<ExpedienteDto?> ObtenerExpedientePorId(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"expediente/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var expediente = await response.Content.ReadFromJsonAsync<ExpedienteDto>(cancellationToken: ct);
                return expediente;
            }
            else
            {
                return null;
            }
        }

        #region CRUD Expediente
        public async Task<(bool Exito, string Mensaje, ExpedienteDto? Datos)> CRUDExpedientes(ExpedienteDto expediente, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "expediente";

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
                    endpoint += $"/{expediente.numero_identificacion}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(expediente);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var expedientecreado = System.Text.Json.JsonSerializer.Deserialize<ExpedienteDto>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", expedientecreado);
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
