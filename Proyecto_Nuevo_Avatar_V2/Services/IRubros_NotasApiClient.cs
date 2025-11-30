using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IRubros_NotasApiClient
    {
        Task<(bool Exito, string Mensaje, DesgloseRubro?)> CargarDesglose(DesgloseRubro desglose, string accessToken, CancellationToken ct = default);
        Task<DesgloseRubro> ObtenerDesglose(string accessToken, string curso, string grupo, CancellationToken ct = default);
        Task<(bool Exito, string Mensaje, Notas?)> AsignarNota(Notas nota, string accion, string accessToken, CancellationToken ct = default);
        Task<List<Notas>?> ObtenerNotas(string accessToken, string curso, string identificacion, CancellationToken ct = default);
    }
}
