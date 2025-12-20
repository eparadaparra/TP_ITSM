using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Execon
{
    public class AccountInfo
    {
        //[JsonPropertyName("RecId")]
        //public string RecId { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("CustID")]
        public string CustID { get; set; }
    }
}
