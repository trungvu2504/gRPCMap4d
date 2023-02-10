namespace gRPCMap4d
{
    /// <summary>
    /// Appsettings
    /// </summary>
    public class AppSetting
    {
        public KeyCloak KeyCloak { get; set; } = default!;
    }

    /// <summary>
    /// Key Cloak
    /// </summary>
    public class KeyCloak
    {
        /// <summary>
        /// Public Key
        /// </summary>
        public string PublicKey { get; set; } = default!;

        /// <summary>
        /// Key Cloak Endpoint
        /// </summary>
        public string KeyCloakEndpoint { get; set; } = default!;

        /// <summary>
        /// Master Endpoint
        /// For authen to get token
        /// </summary>
        public string MasterEndpoint { get; set; } = default!;

        /// <summary>
        /// Max result when request to keyCloak
        /// </summary>
        public byte MaxResultRequest { get; set; }

        /// <summary>
        /// ClientIdMaster
        /// </summary>
        public string ClientIdMaster { get; set; } = default!;
        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; set; } = default!;

        /// <summary>
        /// Password
        /// </summary>
        public string PassWord { get; set; } = default!;

        /// <summary>
        /// Realm
        /// </summary>
        public string Realm { get; set; } = default!;

        /// <summary>
        /// Client Endpoint
        /// </summary>
        public string ClientEndpoint { get; set; } = default!;

        /// <summary>
        /// Client Id
        /// </summary>
        public string ClientId { get; set; } = default!;

        /// <summary>
        /// Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = default!;
    }
}
