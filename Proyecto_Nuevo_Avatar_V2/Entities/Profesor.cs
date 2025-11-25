using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Profesor
    {
        [Required(ErrorMessage = "El ID del profesor es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID no puede superar los 10 caracteres.")]
        [JsonPropertyName("ID_Profesor")]
        public string ID_Profesor { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        [JsonPropertyName("TipoIdentificacion")]
        public string? TipoIdentificacion { get; set; }

        [Required(ErrorMessage = "El número de identificación es obligatorio.")]
        [JsonPropertyName("NumeroIdentificacion")]
        public string? NumeroIdentificacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [RegularExpression(@"^[^@\s]+@cuc\.ac\.cr$", ErrorMessage = "El correo debe pertenecer al dominio cuc.ac.cr.")]
        [JsonPropertyName("Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("FechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[2-9]\d{3}-\d{4}$", ErrorMessage = "Formato inválido. Use ####-####.")]
        [JsonPropertyName("Telefono")]
        public string? Telefono { get; set; }

        [JsonIgnore]
        public string? Mensaje { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
}

