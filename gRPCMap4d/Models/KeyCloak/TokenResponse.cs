using System.Text.Json.Serialization;

namespace gRPCMap4d.Models.KeyCloak;
/// <summary>
/// Token response model
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Token response
    /// </summary>
    public TokenResponse()
    {
        ModifyTime = DateTime.Now;
    }

    /// <summary>
    /// Access token
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    /// <summary>
    /// Refresh token
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = default!;

    /// <summary>
    /// Api domain
    /// </summary>
    [JsonPropertyName("api_domain")]
    public string ApiDomain { get; set; } = default!;

    /// <summary>
    /// Token type
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = default!;

    /// <summary>
    /// Expires in
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Expires in
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// Expires in
    /// </summary>
    [JsonPropertyName("expiresInDateTime")]
    public DateTime ExpiresInDateTime
    {
        get
        {
            var time = ModifyTime.AddSeconds(ExpiresIn);
            return time;
        }
    }

    /// <summary>
    /// Set date create object
    /// </summary>
    private DateTime ModifyTime { get; set; }
}

