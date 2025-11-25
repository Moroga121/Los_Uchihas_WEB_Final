using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Curso
    {
        [Required(ErrorMessage = "El ID del curso es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID no puede superar los 10 caracteres.")]
        [JsonPropertyName("ID_Curso")]
        public string? ID_Curso { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar una carrera.")]
        [JsonPropertyName("ID_Carrera")]
        public string? ID_Carrera { get; set; } = null!;

        [Required(ErrorMessage = "El nivel es obligatorio.")]
        [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12.")]
        [JsonPropertyName("Nivel")]
        public int Nivel { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; } = null!;

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}

