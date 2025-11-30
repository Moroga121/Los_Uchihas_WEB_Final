using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class ExpedienteDto
    {
        public string? numero_identificacion { get; set; }
        public string? tipo_identificacion { get; set; }
        public string? email { get; set; }
        public string? nombre { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public int? id_distrito { get; set; }
        public string? otras_senas { get; set; }
        public string? telefono { get; set; }
        [JsonIgnore]
        public string? Accion { get; set; } = null!;



    }
}
