using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Execon
{
    public class TaskInfo
    {
        [JsonPropertyName("RecId")]
        public string RecId { get; set; }

        [JsonPropertyName("EX_CodigoCierre")]
        public string? EX_CodigoCierre { get; set; }

        [JsonPropertyName("AssignmentID")]
        public long AssignmentID { get; set; }

        [JsonPropertyName("Details")]
        public string? Details { get; set; }

        [JsonPropertyName("ParentLink_Category")]
        public string? ParentLink_Category { get; set; }

        [JsonPropertyName("ParentLink_RecID")]
        public string? ParentLink_RecID { get; set; }

        [JsonPropertyName("Subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("Priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("PlannedStartDate")]
        public DateTimeOffset PlannedStartDate { get; set; }

        [JsonPropertyName("scheduled_date_programming")]
        public string? scheduled_date_programming
        {
            get => PlannedStartDate.ToString("yyyy-MM-dd");
            set { /* Opcional: si necesitas setter */ }
        }

        [JsonPropertyName("scheduled_hour_since")]
        public string? scheduled_hour_since
        {
            get => PlannedStartDate.ToString("HH:mm");
            set { /* Opcional: si necesitas setter */ }
        }

        [JsonPropertyName("scheduled_hour_limit")]
        public string? scheduled_hour_limit
        {
            get => PlannedStartDate.ToString("HH:mm");
            set { /* Opcional: si necesitas setter */ }
        }
    }
    public class TaskCatalogInfo
    {
        [JsonPropertyName("RecId")]
        public string RecId { get; set; }

        [JsonPropertyName("EX_IdActividadTP")]
        public string EX_IdActividadTP { get; set; }

        [JsonPropertyName("TaskSubject")]
        public string TaskSubject { get; set; }
    }
}
