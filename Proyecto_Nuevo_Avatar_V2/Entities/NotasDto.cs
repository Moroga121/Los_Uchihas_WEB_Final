using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class NotasDto
    {
        [JsonPropertyName("codigoCurso")]
        public string Codigo_Curso { get; set; } = string.Empty;
        [JsonPropertyName("nombreCurso")]
        public string Nombre_Curso { get; set; } = string.Empty;
        public double Promedio {  get; set; }
    }

    public class PeriodoDto
    {   
        public string ID_Periodo { get; set; }
        public int Año { get; set; }
        public int Numero_Periodo { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }
        public string Estado { get; set; }
    }
}
