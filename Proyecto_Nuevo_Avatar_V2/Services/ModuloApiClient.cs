using Proyecto_Nuevo_Avatar_V2.Entities;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IModuloApiClient
    {
        Task<List<Modulo>> ObtenerMenuPorRolAsync(string rol, string accessToken, CancellationToken ct = default);
        Task<List<Modulo>?> ObtenerModulosAsync(string accessToken, CancellationToken ct = default);
        Task<List<Rol_Modulo>> ObtenerRol_UsuarioAsync(string accessToken, CancellationToken ct = default);
        Task<Modulo?> ObtenerModuloPorId(string id, string accessToken, CancellationToken ct = default);
        Task<(bool Exito, string Mensaje, Modulo? Datos)> CRUDModulos(Modulo modulos, string accessToken, string accion, CancellationToken ct = default);

        Task<(bool Exito, string Mensaje, Rol_Modulo? Datos)> ActualizarPermisosModuloAsync(
            string moduloId,
            List<string> rolesSeleccionados,
            string accessToken,
            CancellationToken ct = default);

    }
    public class ModuloApiClient : IModuloApiClient
    {
        private readonly HttpClient _http;

        public ModuloApiClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        #region Obtener modulos

        #region Obtener todos los modulos
        public async Task<List<Modulo>?> ObtenerModulosAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, "modulo");
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var modulos = await response.Content.ReadFromJsonAsync<List<Modulo>>(cancellationToken: ct);
                return modulos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener modulos: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Obtener menu por rol para cargar menu
        public async Task<List<Modulo>> ObtenerMenuPorRolAsync(string rol, string accessToken, CancellationToken ct = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"modulo/por-rol/{rol}");
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }

                var modulos = await response.Content.ReadFromJsonAsync<List<Modulo>>(cancellationToken: ct);

                foreach (var modulo in modulos)
                {
                    switch (modulo.Identificador_Modulo.ToString())
                    {
                        case "Admin":
                            modulo.Opciones = new List<Opcion>
                    {
                        new Opcion { Nombre = "Administrar Usuarios", Ruta = "/ADM_Usuarios/Index" },
                        new Opcion { Nombre = "Administrar Roles", Ruta = "/ADM_Roles/Index" },
                        new Opcion { Nombre = "Administrar Parámetros", Ruta = "/ADM_Parametros/Index" },
                        new Opcion { Nombre = "Administrar Módulos", Ruta = "/ADM_Modulos/Index" },
                        new Opcion { Nombre = "Administrar Instituciones", Ruta = "/ADM8_Instituciones/Index" },
                        new Opcion { Nombre = "Administrar Carreras", Ruta = "/ADM9_Carreras/Index" },
                        new Opcion { Nombre = "Administrar Cursos", Ruta = "/ADM10_Cursos/Index" },
                        new Opcion { Nombre = "Administrar Profesores", Ruta = "/ADM11_Profesores/Index" },
                        new Opcion { Nombre = "Administrar Periodos", Ruta = "/ADM12_Periodos/Index" },
                        new Opcion { Nombre = "Administrar Grupos", Ruta = "/ADM13_Grupos/Index" },
                        new Opcion { Nombre = "Gestionar Prematricula", Ruta = "/ADM_Prematricula/Index" },
                        new Opcion { Nombre = "Gestionar Matrícula", Ruta = "/ADM_Matricula/Index" },
                        new Opcion { Nombre = "Administrar Expedientes", Ruta = "/ADM_Expedientes/Index" },
                        new Opcion { Nombre = "Consultar Promedios", Ruta = "/ACA1/Index" },
                        new Opcion { Nombre = "Listar Periodos", Ruta = "/ACA2/Index" },
                        new Opcion { Nombre = "Administrar Facturas", Ruta = "/IPN1/Index" },
                        new Opcion { Nombre = "Registro y Consulta de Pagos", Ruta = "/IPN2/Index" },
                        new Opcion { Nombre = "Notificaciones y Correo", Ruta = "/IPN3/Index" },
                        new Opcion { Nombre = "Consulta Bitácora", Ruta = "/ADM_Bitacora/Bitacora" },

                    };
                            break;

                        case "Prof":
                            modulo.Opciones = new List<Opcion>
                    {
                        new Opcion { Nombre = "Administrar Grupos", Ruta = "/ADM13_Grupos/Index" },
                        new Opcion { Nombre = "Cargar Desglose de Rubros", Ruta = "/PROF_Rubros/Index" },
                        new Opcion { Nombre = "Consultar Promedios", Ruta = "/ACA1/Index" }
                    };
                            break;

                        default:
                            // Si hay otros módulos
                            modulo.Opciones = new List<Opcion>();
                            break;
                    }
                }

                return modulos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener módulos: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Obtener los roles y modulos relacionados 
        public async Task<List<Rol_Modulo>> ObtenerRol_UsuarioAsync(string accessToken, CancellationToken ct = default)
        {
            try
            {
                // Preparar el request sin codificar
                var request = new HttpRequestMessage(HttpMethod.Get, "modulo/por-rol-usuario");
                request.Headers.Add("access_token", accessToken);

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}, contenido: {content}");
                    return null;
                }
                // Leer el contenido JSON
                var modulos = await response.Content.ReadFromJsonAsync<List<Rol_Modulo>>(cancellationToken: ct);
                return modulos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener modulos: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Obtener modulo por id
        public async Task<Modulo?> ObtenerModuloPorId(string id, string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"modulo/por-id/{id}");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var modulo = await response.Content.ReadFromJsonAsync<Modulo>(cancellationToken: ct);
                return modulo;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #endregion

        #region CRUD Modulos
        public async Task<(bool Exito, string Mensaje, Modulo? Datos)> CRUDModulos(Modulo modulos, string accessToken, string accion, CancellationToken ct = default)
        {
            HttpMethod method;
            string endpoint = "modulo";

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
                    endpoint += $"/{modulos.Identificador_Modulo}";
                    break;
                default:
                    throw new ArgumentException("Acción no válida. Use 'Insert', 'Update' o 'Delete'.");
            }

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("access_token", accessToken);

            if (accion is "Insert" or "Update")
                request.Content = JsonContent.Create(modulos);

            var response = await _http.SendAsync(request, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Si la API devuelve el objeto JSON del rol creado/actualizado
                    var moduloCreado = System.Text.Json.JsonSerializer.Deserialize<Modulo>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Operación realizada con éxito.", moduloCreado);
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

        #region Actualizar permisos de roles por modulo
        public async Task<(bool Exito, string Mensaje, Rol_Modulo? Datos)> ActualizarPermisosModuloAsync(
        string moduloId,
        List<string> rolesSeleccionados,
        string accessToken,
        CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "modulo/actualizar-permisos");
            request.Headers.Add("access_token", accessToken);

            // Objeto que se envía al API
            var payload = new
            {
                Identificador_Modulo = moduloId,
                Roles = rolesSeleccionados
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.SendAsync(request, ct);
                var contenido = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    // Deserializar la respuesta exitosa
                    var rolModulo = System.Text.Json.JsonSerializer.Deserialize<Rol_Modulo>(
                        contenido,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return (true, "Permisos actualizados correctamente.", rolModulo);
                }
                else
                {
                    // Manejo de errores
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
