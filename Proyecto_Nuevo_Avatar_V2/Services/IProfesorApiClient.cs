using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IProfesorApiClient
    {
        Task<List<Profesor>> ObtenerProfesoresAsync(string accessToken);
        Task<Profesor?> ObtenerProfesorPorIdAsync(string id, string accessToken);
        Task<(bool Exito, string Mensaje, Profesor? Datos)> CRUDProfesorAsync(Profesor profesor, string accion, string accessToken);
        Task<IEnumerable<Profesor>> BuscarProfesoresAsync(string busqueda, string ordenCampo, string ordenDireccion, int pagina, int tamanoPagina, string token);
    }
}


