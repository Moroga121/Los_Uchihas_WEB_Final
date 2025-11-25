using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IGrupoApiClient
    {
        Task<List<Grupo>?> ObtenerTodosGruposAsync(string accessToken);
        Task<Grupo?> ObtenerGrupoPorIdAsync(string id, string accessToken);
        Task<(bool Exito, string Mensaje, Grupo? Datos)> CRUDGrupoAsync(Grupo grupo, string accion, string accessToken);
    }
}

