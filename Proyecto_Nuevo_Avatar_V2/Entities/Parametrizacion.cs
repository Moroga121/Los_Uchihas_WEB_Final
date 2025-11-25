using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Parametrizacion
    {
        [Required(ErrorMessage = "El identificador es obligatorio.")]
        [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "El identificador solo puede contener letras mayúsculas y números.")]
        [StringLength(10, ErrorMessage = "El identificador no puede tener más de 10 caracteres.")]
        public string Identificador_Parametro { get; set; } = null!;

        [Required(ErrorMessage = "El valor es obligatorio.")]
        [StringLength(500, ErrorMessage = "El valor no puede tener más de 500 caracteres.")]
        public string Valor_Parametro { get; set; } = null!;

        [JsonIgnore]
        public string Mensaje { get; set; } = null!;

        [JsonIgnore]
        public string Accion { get; set; } = null!;
    }
}
