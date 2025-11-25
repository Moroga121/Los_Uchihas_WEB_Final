using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class Pago
    {
        [JsonPropertyName("numero_Pago")]
        public int Numero_Pago { get; set; }

        [JsonPropertyName("numero_Factura")]
        public long Numero_Factura { get; set; }

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        [JsonPropertyName("Estado_Pago")]
        public string Estado_Pago { get; set; }

        [JsonPropertyName("motivo_Reversa")]
        public string Motivo_Reversa { get; set; }

        [JsonPropertyName("fecha_Reversa")]
        public DateTime? Fecha_Reversa { get; set; }

        [JsonPropertyName("fecha_Pago")]
        public DateTime? Fecha_Pago { get; set; }

        [JsonPropertyName("ruta_Comprobante")]
        public string Ruta_Comprobante { get; set; }

        public decimal Monto { get; set; }
    }
}
