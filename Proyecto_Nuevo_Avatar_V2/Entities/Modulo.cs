using System.ComponentModel.DataAnnotations;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Modulo
    {
        [Required(ErrorMessage = "El identificador del módulo no puede estar vacío.")]
        public string Identificador_Modulo { get; set; }

        [Required(ErrorMessage = "El nombre del módulo no puede estar vacío.")]
        public string Nombre_Modulo { get; set; } = string.Empty;
        public string? Ruta { get; set; }

        [Required(ErrorMessage = "El estado no puede estar vacío.")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "El orden no puede estar vacío.")]
        public int Orden { get; set; }

        // Relación con las opciones o submódulos
        public List<Opcion> Opciones { get; set; } = new List<Opcion>();
    }

    public class Opcion
    {
        public string Nombre { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;

    }
}
