using System.Text.Json.Serialization;

namespace TP_ITSM.Models
{
    public class ErrorResponse
    {
        public string Status { get; set; }
        public string Mensaje { get; set; }
        public Object data { get; set; } = new Object() { };
    }

    public class ODataResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? ODataContext { get; set; }

        [JsonPropertyName("@odata.count")]
        public int ODataCount { get; set; }

        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }

    }
}
