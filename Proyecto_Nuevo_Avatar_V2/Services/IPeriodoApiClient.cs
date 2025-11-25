using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IPeriodoApiClient
    {
        Task<List<Periodo>> ObtenerTodosAsync(string accessToken);
        Task<Periodo?> ObtenerPorIdAsync(string id, string accessToken);
        Task<(bool Exito, string Mensaje, Periodo? Datos)> CRUDPeriodoAsync(Periodo periodo, string accion, string accessToken);
    }
}

