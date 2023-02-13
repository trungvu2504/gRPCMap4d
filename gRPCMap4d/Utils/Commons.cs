using Grpc.Core;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace gRPCMap4d.Utils
{
    public static class Commons
    {
        /// <summary>
        /// Serialize options
        /// </summary>
        public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Get status error service
        /// </summary>
        public static Status ErrorExceptionServiceStatus => new(StatusCode.Unavailable, Constants.Exception);

    }
}
