using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface IMatriculaApiClient
    {

        Task<List<MatriculaCompletaDto>?> Obtener_Todas_Matriculas(string accessToken, CancellationToken ct = default);
        Task<MatriculaDto?> ObtenerMatriculaPorId(int id, string accessToken, CancellationToken ct = default);

        Task<List<MatriculaCompletaDto>?> Obtener_Matriculados_Por_Curso_Grupo(string curso, string grupo, string accessToken, CancellationToken ct = default);

        Task<(bool Exito, string Mensaje, MatriculaDto? Datos)> CRUDMatricula(MatriculaDto matricula, string accessToken, string accion, CancellationToken ct = default);
    }


}
