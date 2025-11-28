namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class DireccionesDto
    {
    }

    public class ProvinciaDto
    {
        public int id_provincia { get; set; }
        public string? nombre { get; set; }
    }

    public class CantonDto
    {
        public int id_canton { get; set; }
        public string? provincia { get; set; }
        public string? nombre { get; set; }
    }

    public class DistritoDto
    {
        public int id_distrito { get; set; }
        public string? nombre_provincia { get; set; }
        public string? nombre_canton { get; set; }
        public string? nombre_distrito { get; set; }
    }

}
