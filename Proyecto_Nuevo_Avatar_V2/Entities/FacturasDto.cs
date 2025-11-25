using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class FacturasDto
    {
        public long Numero_Factura { get; set; }

        public string Identificacion { get; set; } = string.Empty;
        public int MontoBase { get; set; }
        public decimal IVA { get; set; }
        [JsonPropertyName("montoTotal")]
        public decimal MontoTotal { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public DateTime Fecha_Creacion {get; set;}
        public string Estado_Factura { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime Fecha_Reversa { get; set; }

    }

    public class MatriculaDto
    {
        public int Id_Matricula { get; set; }
        public string Numero_Identificacion { get; set; }
        public string Curso { get; set; }
        public string Grupo { get; set; }
        public string Id_Periodo { get; set; }
    }

    public class DetalleFacturaDto
    {
        [JsonPropertyName("iD_Detalle_Factura")]
        public int ID_Detalle_Factura { get; set; }

        [JsonPropertyName("nombre_Curso")]
        public string Nombre_Curso { get; set; } = string.Empty;

        [JsonPropertyName("montoBase")]
        public decimal MontoBase { get; set; }

        [JsonPropertyName("iva")]
        public decimal IVA { get; set; }

        [JsonPropertyName("montoTotal")]
        public decimal MontoTotal { get; set; }
    }

}
