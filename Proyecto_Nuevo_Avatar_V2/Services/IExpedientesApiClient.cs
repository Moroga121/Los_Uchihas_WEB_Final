using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IExpedientesApiClient
    {

        public Task<List<ExpedienteDto>?> Obtener_Todos_Expedientes(string accessToken, CancellationToken ct = default);
        public Task<ExpedienteDto?> ObtenerExpedientePorId(string id, string accessToken, CancellationToken ct = default);
        public Task<(bool Exito, string Mensaje, ExpedienteDto? Datos)> CRUDExpedientes(ExpedienteDto expediente, string accessToken, string accion, CancellationToken ct = default);


    }
}
