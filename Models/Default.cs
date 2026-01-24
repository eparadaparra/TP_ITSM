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

    public class NotaITSM
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("objectNumber")]
        public string objectNumber { get; set; }

        [JsonPropertyName("subject")]
        public string subject { get; set; }

        [JsonPropertyName("note")]
        public string note { get; set; }

        [JsonPropertyName("category")]
        public string category { get; set; } = "Memo";

        [JsonPropertyName("source")]
        public string source { get; set; } = "Other";
    }
}
