using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Prematricula
    {
            public int id_prematricula { get; set; }
            public string? numero_identificacion { get; set; }
            public string? carrera { get; set; }
            public string? curso { get; set; }
            public string? observaciones { get; set; }
            public string? Id_Periodo { get; set; }
            [JsonIgnore]
            public string Accion { get; set; } = null!;

    }
}
