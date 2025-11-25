using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IParametrosApiClient
    {

        Task<(bool Exito, string Mensaje, Parametrizacion? Datos)> CRUDParametros(Parametrizacion parametros, string accessToken, string accion, CancellationToken ct = default);

        Task<List<Parametrizacion>?> ObtenerParametrosAsync(string accessToken, CancellationToken ct = default);

        Task<Parametrizacion?> ObtenerParametrolPorId(string id, string accessToken, CancellationToken ct = default);

    }
}
