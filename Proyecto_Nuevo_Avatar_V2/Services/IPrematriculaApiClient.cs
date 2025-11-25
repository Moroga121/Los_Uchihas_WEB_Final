namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IPrematriculaApiClient
    {
        public Task<List<Entities.Prematricula>?> Obtener_Todas_Prematriculas(string accessToken, CancellationToken ct = default);
        public Task<Entities.Prematricula?> ObtenerPrematriculaPorId(string id, string accessToken, CancellationToken ct = default);
        public Task<(bool Exito, string Mensaje, Entities.Prematricula? Datos)> CRUDPrematricula(Entities.Prematricula prematricula, string accessToken, string accion, CancellationToken ct = default);
    }
}
