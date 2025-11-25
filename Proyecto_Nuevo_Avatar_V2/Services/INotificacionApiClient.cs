using System.Net;
using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface INotificacionApiClient
    {

        Task<List<Notificacion2Dto>> ObtenerNotificacionesAsync(string email, string accessToken, CancellationToken ct = default);
        Task<List<NotificacionHistorialDto>> ObtenerNotificaciones(string accessToken, CancellationToken ct = default);
        Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(NotificacionDto notificacion, string accessToken, CancellationToken ct = default);
    }
}
