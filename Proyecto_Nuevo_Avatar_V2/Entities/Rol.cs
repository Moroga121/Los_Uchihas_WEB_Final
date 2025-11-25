using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Rol
    {
        [Required(ErrorMessage = "El identificador del rol es obligatorio.")]
        [JsonPropertyName("Identificador_Rol")]
        public string Identificador_Rol { get; set; } = null!;
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [JsonPropertyName("Nombre_Rol")]
        public string Nombre_Rol { get; set; } = null!;

        [JsonIgnore]
        public string Mensaje { get; set; } = null!;

        [JsonIgnore]
        public string Accion { get; set; } = null!;

    }

    public class Rol_Modulo
    {
        public int ID_Rol_Usuario { get; set; }
        [JsonPropertyName("Identificador_Rol")]
        public string Identificador_Rol { get; set; } = null!;
        [JsonPropertyName("Identificador_Modulo")]
        public string Identificador_Modulo { get; set; } = null!;

        public List<string> Modulos { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }
}
