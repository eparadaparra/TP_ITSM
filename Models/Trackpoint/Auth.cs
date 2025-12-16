using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Trackpoint
{
    public class TpAuthRequest
    {
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("password")]
        public string password { get; set; }
    }

    public class TpAuthResponse
    {
        public string statusCode { get; set; }
        public string message { get; set; }
        public string token { get; set; }
        public string uuid { get; set; }
        public string user { get; set; }
    }
}
