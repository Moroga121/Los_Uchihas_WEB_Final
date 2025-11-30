using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Notas
    {

        public int id_nota { get; set; }
        public string? numero_identificacion { get; set; }
        public string? id_rubro { get; set; }
        public decimal? valor { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
    public class NotasResponse
    {
        public string mensaje { get; set; }
        public List<Notas> data { get; set; }
    }
}
