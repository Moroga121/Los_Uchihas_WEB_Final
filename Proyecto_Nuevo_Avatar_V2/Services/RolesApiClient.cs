using Proyecto_Nuevo_Avatar_V2.Entities;
using System;
using System.Data;
using System.Text;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class RolesApiClient : IRolesApiClient
    {

        private readonly HttpClient _http;
        public RolesApiClient(HttpClient http)
        {
            _http = http;
        }

        #region CRUD ROLES
        public async Task<(bool Exito, string Mensaje, Rol? Datos)> CRUDRoles(Rol roles, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "rol";

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
                    endpoint += $"/{roles.Identificador_Rol}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(roles);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var rolCreado = System.Text.Json.JsonSerializer.Deserialize<Rol>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", rolCreado);
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

        #region Obtener roles
        public async Task<List<Rol>?> ObtenerRolesAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, "rol");
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var roles = await response.Content.ReadFromJsonAsync<List<Rol>>(cancellationToken: ct);
                return roles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener roles: {ex.Message}");
                return null;
            }
        }
        public async Task<Rol?> ObtenerRolPorId(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"rol/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var rol = await response.Content.ReadFromJsonAsync<Rol>(cancellationToken: ct);
                return rol;
            }
            else
            {
                return null;
            }
        }


        #endregion

        #region Actualizar permisos de módulos por rol
        public async Task<(bool Exito, string Mensaje, Rol_Modulo? Datos)> ActualizarPermisosAsync(
        string rolId,
        List<string> modulos,
        string accessToken,
        CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "rol/actualizar-permisos");
            request.Headers.Add("access_token", accessToken);

            var modulosrelacionados = new
            {
                Identificador_Rol = rolId,
                Modulos = modulos
            };

            var json = System.Text.Json.JsonSerializer.Serialize(modulosrelacionados);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // La API devuelve JSON
                    var rolPermisos = System.Text.Json.JsonSerializer.Deserialize<Rol_Modulo>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", rolPermisos);
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
                return (false, $"Error al procesar la respuesta: {ex.Message}", null);
            }
        }

    }
    #endregion

}

