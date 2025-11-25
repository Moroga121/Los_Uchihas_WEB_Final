using System.Text.Json.Serialization;

namespace Proyecto_Nuevo_Avatar_V2.Entities
{
    public class TokenDto
    {

        [JsonPropertyName("access_token")]
        public string Access_Token { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string Refresh_Token { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public DateTime Expires_In { get; set; }

        [JsonPropertyName("usuarioID")]
        public string? Email { get; set; }

        public DateTime RefreshTokenExpires { get; set; }

    }
}
