using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Grupo
    {
        [Required(ErrorMessage = "El ID del grupo es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID no puede superar los 10 caracteres.")]
        [JsonPropertyName("ID_Grupo")]
        public string? ID_Grupo { get; set; }

        [Required(ErrorMessage = "El número de grupo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de grupo debe ser mayor que 0.")]
        [JsonPropertyName("Numero_Grupo")]
        public int? Numero_Grupo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un curso.")]
        [JsonPropertyName("ID_Curso")]
        public string? ID_Curso { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un profesor.")]
        [JsonPropertyName("ID_Profesor")]
        public string? ID_Profesor { get; set; }

        [Required(ErrorMessage = "Debe indicar un horario.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúñÑ\s]+ \d{2}:\d{2}-\d{2}:\d{2}$",
            ErrorMessage = "El horario debe tener formato válido. Ejemplo: 'Lunes 08:00-10:00'.")]
        [JsonPropertyName("Horario")]
        public string? Horario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [JsonPropertyName("ID_Periodo")]
        public string? ID_Periodo { get; set; }

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}

