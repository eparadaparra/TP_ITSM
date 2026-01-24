using Newtonsoft.Json;

namespace TP_ITSM.Models.Execon
{
    [JsonConverter(typeof(ParentInfoNewtonsoftConverter))]
    public class ParentInfo
    {
        [JsonProperty("RecId")]
        public string? RecId { get; set; }

        [JsonProperty("Owner_Valid")]
        public string? Owner_Valid { get; set; }

        [JsonProperty("Owner")]
        public string? Owner { get; set; }

        // Se llena dinámicamente
        public long ParentNumber { get; set; }

        [JsonProperty("Subject")]
        public string? Subject { get; set; }

        [JsonProperty("EX_LocationID_Link_RecID")]
        public string? EX_LocationID_Link_RecID { get; set; }

        [JsonProperty("EX_CustID_Link_RecID")]
        public string? EX_CustID_Link_RecID { get; set; }

        [JsonProperty("CreatedDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }
    }
}