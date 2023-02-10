using System.Text.Json.Serialization;

namespace gRPCMap4d.Models.KeyCloak
{
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Type
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Value new password
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        /// <summary>
        /// Temporary
        /// </summary>
        [JsonPropertyName("temporary")]
        public bool Temporary { get; set; }
    }
}
