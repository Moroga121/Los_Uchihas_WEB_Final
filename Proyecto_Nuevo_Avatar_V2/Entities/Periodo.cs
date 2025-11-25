using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Periodo
    {
        [Required(ErrorMessage = "El ID del periodo es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID no puede superar los 10 caracteres.")]
        [JsonPropertyName("id_Periodo")]
        public string ID_Periodo { get; set; } = null!;

        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(2000, 2100, ErrorMessage = "El año debe ser del 2000 en adelante.")]
        [JsonPropertyName("año")]
        public int Año { get; set; } = DateTime.Now.Year;

        [Required(ErrorMessage = "El número de periodo es obligatorio.")]
        [Range(1, 4, ErrorMessage = "El número de periodo debe estar entre 1 y 4.")]
        [JsonPropertyName("numero_Periodo")]
        public int Numero_Periodo { get; set; }

        [Required(ErrorMessage = "Debe ingresar una fecha de inicio.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("fecha_Inicio")]
        public DateTime Fecha_Inicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Debe ingresar una fecha de fin.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("fecha_Fin")]
        public DateTime Fecha_Fin { get; set; } = DateTime.Today;

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}

