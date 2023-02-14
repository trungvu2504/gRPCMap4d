using System.Text.Json.Serialization;

namespace gRPCMap4d.Models.KeyCloak
{
    /// <summary>
    /// Update account request keycloak
    /// </summary>
    public class UpdateAccountRequest
    {
        /// <summary>
        /// LastName
        /// </summary>
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        /// <summary>
        /// FirstName
        /// </summary>
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        [JsonPropertyName("attributes")]
        public Attributes? Attributes { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [JsonPropertyName("attributes")]
        public string? PhoneNumber { get; set; }
    }
    /// <summary>
    /// Attributes
    /// </summary>
    public class Attributes
    {
        /// <summary>
        /// List Provinces
        /// </summary>
        [JsonPropertyName("province")]
        public IList<string>? Province { get; set; }

        /// <summary>
        /// List PhoneNumbers
        /// </summary>
        [JsonPropertyName("phone")]
        public IList<string>? Phone { get; set; }
    }
}
