using System.ComponentModel.DataAnnotations;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@cuc(\.ac)?\.cr$", ErrorMessage = "Solo se permiten correos del dominio cuc.ac.cr o cuc.cr.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contrasena { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

    }
}
