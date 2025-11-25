using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Notificacion2Dto
    {
        [JsonPropertyName("fechaEnvio")]
        public DateTime FechaEnvio { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("asunto")]
        public string Asunto { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;


    }

    public class NotificacionDto
    {
        [JsonIgnore]
        public int ID_Notificacion_Correo { get; set; }

        [Required(ErrorMessage = "Correo(s) Destinatarios son Obligatorios")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Asunto del Correo es Obligatorio")]
        [JsonPropertyName("asunto")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Cuerpo del Correo es Obligatorio")]
        [JsonPropertyName("cuerpo")]
        public string Cuerpo { get; set; } = string.Empty;

        [JsonIgnore]
        public string FechaEnvio { get; set; } = string.Empty;

        [JsonIgnore]
        public string? Estado_Envios { get; set; } = string.Empty;
    }

    public class NotificacionHistorialDto
    {
        [JsonPropertyName("fechaEnvio")]
        public DateTime FechaEnvio { get; set; }

        [JsonPropertyName("email")]
        public string Email_Destinatario { get; set; } = string.Empty;

        [JsonPropertyName("asunto")]
        public string Asunto { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
