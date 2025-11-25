using Proyecto_Nuevo_Avatar_V2.Entities;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class LoginApiClient : ILoginApiClient
    {

        private readonly HttpClient _http;
        private readonly IBitacoraApiClient _bitacoraApiClient;

        public LoginApiClient(HttpClient http, IBitacoraApiClient bitacoraApiClient)
        {
            _http = http;
            _bitacoraApiClient = bitacoraApiClient;
        }

        #region "Login"

        public async Task<(bool ok, TokenDto? token, string? mensaje)> LoginAsync(LoginDto login, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "login");
            request.Headers.Add("email", login.Email);
            request.Headers.Add("contrasena", login.Contrasena);

            var response = await _http.SendAsync(request, ct);

            string? msg = null;
            TokenDto? token = null;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    token = await response.Content.ReadFromJsonAsync<TokenDto>(cancellationToken: ct);
                }
                else
                {
                    var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>(cancellationToken: ct);
                    if (payload != null && payload.TryGetValue("mensaje", out var m))
                    {
                        msg = m;
                    }
                }
            }
            catch
            {
                msg = "Error al procesar la respuesta de la API";
            }

            return (response.IsSuccessStatusCode, token, msg);
        }

        #endregion

        #region "Refresh"

        public async Task<TokenDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            

            using var request = new HttpRequestMessage(HttpMethod.Post, "login/refresh");
            request.Headers.TryAddWithoutValidation("refresh_token", refreshToken);

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                

                try
                {
                    var token = System.Text.Json.JsonSerializer.Deserialize<TokenDto>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return token;
                }
                catch (Exception ex)
                {
                    
                    return null;
                }
            }

            return null;

        }

        #endregion

        #region "Validate"

        public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "login/validate");
            request.Headers.Add("access_token", accessToken);

            var response = await _http.SendAsync(request, ct);

            return response.IsSuccessStatusCode;
        }

        #endregion

    }
}
