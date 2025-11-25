using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class UsuarioDto
    {
        [Required(ErrorMessage = "La Identificación no puede estar vacía.")]
        [JsonPropertyName("identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Tipo de Identificación no puede estar vacía.")]
        [JsonPropertyName("tipo_Identificacion")]
        public string Tipo_Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Nombre no puede estar vacío.")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Email no puede estar vacío.")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Contraseña no puede estar vacía.")]
        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Rol no puede estar vacío.")]
        [JsonPropertyName("rol_Usuario")]
        public string Rol_Usuario { get; set; } = string.Empty;
        [JsonPropertyName("Nombre_Rol")]
        public string Nombre_Rol { get; set; } = string.Empty;

        [JsonPropertyName("accion")]
        public string Accion { get; set; } = string.Empty;


    }



    public class Tipos_Identificacion
    {
        [JsonPropertyName("iD_Identificacion")]
        public string ID_Identificacion { get; set; } = null!;


        [JsonPropertyName("tipo_Identificacion")]
        public string Tipo_Identificacion { get; set; } = null!;



    }


    public class ContrasenaSettings
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecialCharacter { get; set; }
    }



}
