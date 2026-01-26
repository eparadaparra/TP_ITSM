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

    public class AttachementITSM
    {
        [JsonPropertyName("Attachment")]
        public AttachInfo Attachment { get; set; }
    }

    public class AttachInfo
    {
        [JsonPropertyName("ParentLink")]
        public string ParentLink { get; set; }

        [JsonPropertyName("ATTACHNAME")]
        public string ATTACHNAME { get; set; }

        [JsonPropertyName("URL")]
        public string URL { get; set; }

        [JsonPropertyName("SaveType")]
        public string SaveType { get; set; } = "Database";

        [JsonPropertyName("Uploaded")]
        public int Uploaded { get; set; } = 1;
    }
}
