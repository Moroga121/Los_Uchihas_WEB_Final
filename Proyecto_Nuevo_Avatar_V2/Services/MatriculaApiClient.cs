using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class MatriculaApiClient: IMatriculaApiClient
    {

        private readonly HttpClient _http;
        public MatriculaApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MatriculaCompletaDto>?> Obtener_Matriculados_Por_Curso_Grupo(string curso, string grupo, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"matricula/curso-grupo?curso={Uri.EscapeDataString(curso)}&grupo={Uri.EscapeDataString(grupo)}");

            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<MatriculaCompletaDto>>(cancellationToken: ct);
        }


        public async Task<List<MatriculaCompletaDto>?> Obtener_Todas_Matriculas(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "matricula");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<List<MatriculaCompletaDto>>(cancellationToken: ct);
        }

        public async Task<MatriculaDto?> ObtenerMatriculaPorId(int id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"matricula/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<MatriculaDto>(cancellationToken: ct);
        }

        public async Task<(bool Exito, string Mensaje, MatriculaDto? Datos)> CRUDMatricula(MatriculaDto matricula, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "matricula";

            switch (accion)
            {
                case "Crear":
                    method = HttpMethod.Post;
                    break;
                case "Actualizar":
                    method = HttpMethod.Put;
                    break;
                case "Eliminar":
                    method = HttpMethod.Delete;
                    endpoint += $"/{matricula.Id_Matricula}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Crear', 'Actualizar' o 'Eliminar'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion == "Crear" || accion == "Actualizar")
                request.Content = JsonContent.Create(matricula);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var matriculaCreada = System.Text.Json.JsonSerializer.Deserialize<MatriculaDto>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", matriculaCreada);
                }
                else
                {
                    var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
                    if (error != null && error.TryGetValue("mensaje", out var msg))
                        return (false, msg, null);

                    return (false, $"Error desconocido ({response.StatusCode})", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error procesando la respuesta: {ex.Message}", null);
            }
        }


    }
}
