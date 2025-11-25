using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface ICursoApiClient
    {
        Task<List<Curso>> ObtenerTodosAsync(string accessToken);
        Task<Curso?> ObtenerPorIdAsync(string id, string accessToken);
        Task<(bool Exito, string Mensaje, Curso? Datos)> CRUDCursoAsync(Curso curso, string accion, string accessToken);
    }
}

