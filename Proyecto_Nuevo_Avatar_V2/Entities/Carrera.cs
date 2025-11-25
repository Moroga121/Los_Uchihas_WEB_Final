using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Carrera
    {
        [Required(ErrorMessage = "El ID de la carrera es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID no puede superar los 10 caracteres.")]
        [JsonPropertyName("ID_Carrera")]
        public string? ID_Carrera { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar una institución.")]
        [JsonPropertyName("ID_Institucion")]
        public string? ID_Institucion { get; set; } = null!;

        [Required(ErrorMessage = "Debe ingresar un ID de director.")]
        [JsonPropertyName("ID_Director")]
        public string? ID_Director { get; set; } = null!;

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}


