using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Rubros
    {
        public string id_rubro { get; set; }
        public string? nombre { get; set; }
        public decimal porcentaje { get; set; }
    }

    public class DesgloseRubro
    {
        public string? nombre_grupo { get; set; }
        public string? nombre_curso { get; set; }
        public List<Rubros>? rubros { get; set; }

        [JsonIgnore]
        public string? Accion { get; set; }
    }
    public class DesgloseResponse
    {
        public string mensaje { get; set; }
        public DesgloseRubro data { get; set; }
    }

}
