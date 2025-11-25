using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Institucion
    {
        [Required(ErrorMessage = "El identificador de la institución es obligatorio.")]
        [StringLength(5, ErrorMessage = "El identificador no puede exceder los 5 caracteres.")]
        [JsonPropertyName("ID_Institucion")]
        public string ID_Institucion { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de la institución es obligatorio.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; } = null!;

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}
