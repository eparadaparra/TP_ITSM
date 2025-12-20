
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Trackpoint
{
    public class TpCustIdRequest
    {
        public string? client_id { get; set; }
    }

    public class FirebaseIdRequest
    {
        public string? id { get; set; }

    }

    public partial class TpCustomerResponse
    {
        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonPropertyName("message")]
        public string? message { get; set; }

        [JsonPropertyName("data")]
        public DataCustomerTP? data { get; set; }

        [JsonPropertyName("id")]
        public string? id { get; set; }
    }

    public partial class DataCustomerTP
    {
        [JsonPropertyName("id")]
        public string? id { get; set; }

        [JsonPropertyName("user")]
        public string? user { get; set; }

        [JsonPropertyName("user_uuid_id")]
        public string? user_uuid_id { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonPropertyName("contact")]
        public string? contact { get; set; }

        [JsonPropertyName("mail")]
        public string? mail { get; set; }

        [JsonPropertyName("telephone")]
        public string? telephone { get; set; }

        [JsonPropertyName("address")]
        public string? address { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset? date { get; set; } //DateTimeOffset

        [JsonPropertyName("client_id")]
        public string? client_id { get; set; }

        [JsonPropertyName("modules_notify")]
        public ModulesNotify? modules_notify { get; set; } = null;
    }

    public partial class ModulesNotify
    {
        [JsonPropertyName("email")]
        public List<string> email { get; set; } = [];

        [JsonPropertyName("notificacion")]
        public Notificacion notificacion { get; set; }

        [JsonPropertyName("filters_uid")]
        public string? filters_uid { get; set; }

        [JsonPropertyName("firebase_uid")]
        public string? firebase_uid { get; set; }

        [JsonPropertyName("type_id")]
        public long type_id { get; set; }

        [JsonPropertyName("status")]
        public bool status { get; set; }
    }

    public partial class Notificacion
    {
        [JsonPropertyName("init")]
        public bool init { get; set; } = false;

        [JsonPropertyName("close")]
        public bool close { get; set; } = false;

        [JsonPropertyName("scheduled")]
        public bool scheduled { get; set; } = false;

        [JsonPropertyName("daybefore")]
        public bool daybefore { get; set; } = false;

        [JsonPropertyName("send_ics")]
        public bool send_ics { get; set; } = false;
    }


    public partial class TpCustomerList
    {
        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonPropertyName("message")]
        public string? message { get; set; }

        [JsonPropertyName("data")]
        public List<DataCustomerTP> data { get; set; }
    }
}
 