using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class UsuarioApiClient : IUsuarioApiClient
    {

        private readonly HttpClient _http;

        public UsuarioApiClient(HttpClient http)
        {
            _http = http;
        }

        #region "Cambiar Contraseña"

        public async Task<(bool Exito, string Mensaje, UsuarioDto? Datos)> CambiarContrasenaAsync(UsuarioDto usuario, string accessToken, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "usuario/cambiar_contrasena");
            request.Headers.Add("access_token", accessToken);

            // Solo enviar email y contrasena
            var body = new { email = usuario.Email, contrasena = usuario.Contrasena };
            request.Content = JsonContent.Create(body);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Deserializar como diccionario genérico
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        contenido,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (result != null)
                    {
                        // Obtener mensaje
                        string mensaje = result.ContainsKey("mensaje") ? result["mensaje"]?.ToString() ?? "" : "Contraseña actualizada correctamente";

                        // Obtener usuario actualizado
                        UsuarioDto? usuarioActualizado = null;
                        if (result.ContainsKey("usuario") && result["usuario"] is JsonElement usuarioJson)
                        {
                            usuarioActualizado = usuarioJson.Deserialize<UsuarioDto>(
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                            );
                        }

                        return (true, mensaje, usuarioActualizado);
                    }

                    return (true, "Contraseña actualizada correctamente", null);
                }
                else
                {
                    // Manejar errores tipo { "mensaje": "..." }
                    var error = JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
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

        #region "Obtener Usuario por ID"


        public async Task<UsuarioDto?> ObtenerUsuarioPorIdentificacionAsync(string id, string accessToken, CancellationToken ct = default)
        {


            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, $"usuario/id/{id}");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                // Enviar la petición
                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }

                // Leer el contenido JSON

                var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>(cancellationToken: ct);
                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<UsuarioDto?> ObtenerUsuarioPorCorreoAsync(string email, string accessToken, CancellationToken ct = default)
        {


            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, $"usuario/{email}");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                // Enviar la petición
                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }

                // Leer el contenido JSON
                var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>(cancellationToken: ct);
                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener usuario: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region "Obtener Usuarios Filtrados"

        public async Task<List<UsuarioDto>> ObtenerUsuariosFiltradosAsync(string identificacion, string nombre, string rol, string tipo, string dominio, string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Construir la URL con los parámetros (vacíos si no se usan)

                var url = $"usuario/filtrar?identificacion={identificacion}&nombre={nombre}&rol={rol}&tipo={tipo}&dominio={dominio}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("access_token", accessToken);



                var response = await _http.SendAsync(request, ct);


                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<UsuarioDto>();
                }

                // Leer y deserializar el JSON

                var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>(cancellationToken: ct);
                return usuarios ?? new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener usuarios filtrados: {ex.Message}");
                return new List<UsuarioDto>();
            }
        }

        #endregion

        #region "Obtener Tipos de Identificacion"
        public async Task<List<Tipos_Identificacion>> ObtenerTiposIdentificacion(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request

                var request = new HttpRequestMessage(HttpMethod.Get, "usuario/tipos_identificacion");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                // Enviar la petición
                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<Tipos_Identificacion>();
                }

                // Leer el contenido JSON

                var usuarios = await response.Content.ReadFromJsonAsync<List<Tipos_Identificacion>>(cancellationToken: ct);
                return usuarios ?? new List<Tipos_Identificacion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener tipo de identificacion: {ex.Message}");
                return new List<Tipos_Identificacion>();
            }
        }

        #endregion

        #region "Obtener Dominios"

        public async Task<List<UsuarioDto>> ObtenerDominios(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request
                var request = new HttpRequestMessage(HttpMethod.Get, "usuario/dominios");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API
                // Enviar la petición
                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<UsuarioDto>();
                }
                // Leer el contenido JSON
                var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>(cancellationToken: ct);
                return usuarios ?? new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener dominios: {ex.Message}");
                return new List<UsuarioDto>();
            }
        }

        #endregion

        #region "Obtener Todos los Usuarios"
        public async Task<List<UsuarioDto>> ObtenerTodoslosUsuarios(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request
                var request = new HttpRequestMessage(HttpMethod.Get, "usuario");
                request.Headers.Add("access_token", accessToken); // Header exacto que espera la API

                // Enviar la petición
                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return new List<UsuarioDto>();
                }

                // Leer el contenido JSON

                var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>(cancellationToken: ct);
                return usuarios ?? new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener roles: {ex.Message}");
                return new List<UsuarioDto>();
            }
        }


        #endregion

        #region "Crud Usuarios"



        public async Task<(bool Exito, string Mensaje, UsuarioDto? Datos)> CRUDUsuarios(UsuarioDto usuarios, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "usuario";

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
                    endpoint += $"/{usuarios.Identificacion}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(usuarios);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var usuariocreado = System.Text.Json.JsonSerializer.Deserialize<UsuarioDto>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", usuariocreado);
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
