using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Execon
{
    public class EmployeeInfo
    {
        //[JsonPropertyName("RecId")]
        //public string RecId { get; set; }

        [JsonPropertyName("Department")]
        public string Department { get; set; }

        [JsonPropertyName("Disabled")]
        public bool Disabled { get; set; }

        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("Supervisor")]
        public string Supervisor { get; set; }

        [JsonPropertyName("Team")]
        public string Team { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }
    }
}
