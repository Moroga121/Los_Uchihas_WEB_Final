using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public interface ILoginApiClient
    {

        Task<(bool ok, TokenDto? token, string? mensaje)> LoginAsync(LoginDto login, CancellationToken ct = default);

        Task<TokenDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

        Task<bool> ValidateTokenAsync(string accessToken, CancellationToken ct = default);

    }
}
