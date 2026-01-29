using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Execon
{
    public class ResponseTaskTP
    {
        public void Initialize()
        {
            data = new Data();
        }

        public string status { get; set; } = "No disponible";

        public string message { get; set; } = "No disponible";

        public Data data { get; set; } = new Data();
    }

    public class Data
    {
        public void Initialize()
        {
            start_date_utc  ??= new Timestamp();
            end_date_utc    ??= new Timestamp();
            statusInfo      ??= new StatusInfo();
            preload         ??= new List<Preload>();
            geolocation     ??= new Geolocation();
            modules_config  ??= new ModulesConfig();
            elements        ??= new List<Element>();
        }

        #region Variables simples
        [JsonPropertyName("created_api")]
        public bool created_api { get; set; } = false;
        
        [JsonPropertyName("firebase_id")]
        public string firebase_id { get; set; } = "No disponible";

        [JsonPropertyName("status")]
        public string status { get; set; } = "No disponible";

        [JsonPropertyName("user_name")]
        public string user_name { get; set; } = "No disponible";

        [JsonPropertyName("scheduled_user_email")]
        public string scheduled_user_email { get; set; } = "No disponible";

        [JsonPropertyName("order_number")]
        public string order_number { get; set; } = "No disponible";

        [JsonPropertyName("scheduled_date_programming")]
        public string scheduled_date_programming { get; set; } = "No disponible";

        [JsonPropertyName("scheduled_instructions")]
        public string scheduled_instructions { get; set; } = "No disponible";

        [JsonPropertyName("scheduled_address")]
        public string scheduled_address { get; set; } = "No disponible";

        [JsonPropertyName("scheduled_periodicity")]
        public string scheduled_periodicity { get; set; } = "notrepeat";

        [JsonPropertyName("classification_category_name")]
        public string classification_category_name { get; set; } = "No disponible";

        [JsonPropertyName("classification_subcategory_name")]
        public string classification_subcategory_name { get; set; } = "No disponible";

        [JsonPropertyName("address_checkIn")]
        public string address_checkIn { get; set; } = "No disponible";

        [JsonPropertyName("deletedBy")]
        public string deletedBy { get; set; } = "No disponible";
        #endregion

        #region Variables con Timestamp y DateTimeOffset
        [JsonPropertyName("start_date_utc")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp start_date_utc { get; set; }

        [JsonPropertyName("start_date_utc_dateTimeOffset")]
        public DateTimeOffset start_date_utc_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(start_date_utc._seconds)
                    .AddTicks(start_date_utc._nanoseconds / 100);
        }

        [JsonPropertyName("start_date_utc2")]
        public DateTimeOffset? start_date_utc2 { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("end_date_utc")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp end_date_utc { get; set; }

        [JsonPropertyName("end_date_utc_dateTimeOffset")]
        public DateTimeOffset end_date_utc_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(end_date_utc._seconds)
                    .AddTicks(end_date_utc._nanoseconds / 100);
        }

        [JsonPropertyName("end_date_utc2")]
        public DateTimeOffset? end_date_utc2 { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("scheduled_programming_date")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp scheduled_programming_date { get; set; }

        [JsonPropertyName("scheduled_programming_dateTimeOffset")]
        public DateTimeOffset scheduled_programming_dateTimeOffset {
            get => DateTimeOffset.FromUnixTimeSeconds(scheduled_programming_date._seconds)
                    .AddTicks(scheduled_programming_date._nanoseconds / 100);
        }

        [JsonPropertyName("scheduled_limit_date")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp scheduled_limit_date { get; set; }

        [JsonPropertyName("scheduled_limit_dateTimeOffset")]
        public DateTimeOffset scheduled_limit_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(scheduled_limit_date._seconds)
                    .AddTicks(scheduled_limit_date._nanoseconds / 100);
        }
        #endregion

        #region Variables Complejas
        [JsonPropertyName("statusInfo")]
        public StatusInfo? statusInfo { get; set; } = new StatusInfo();

        [JsonPropertyName("preload")]
        public List<Preload> preload { get; set; }

        [JsonPropertyName("geolocation")]
        public Geolocation geolocation { get; set; } = new Geolocation();

        [JsonPropertyName("modules_config")]
        public ModulesConfig modules_config { get; set; } = new ModulesConfig();

        [JsonPropertyName("elements")]
        public List<Element> elements { get; set; } = new List<Element>();
        #endregion

    }

    public partial class Timestamp
    {
        [JsonPropertyName("_seconds")]
        public long _seconds { get; set; } = 0;

        [JsonPropertyName("_nanoseconds")]
        public long _nanoseconds { get; set; } = 0;
    }

    public partial class StatusInfo
    {
        [JsonPropertyName("txt")]
        public string txt { get; set; } = "Programada";

        [JsonPropertyName("color")]
        public string color { get; set; } = "#118AB2";
    }

    public partial class Preload
    {
        [JsonPropertyName("frmRecIdTask")]
        public string frmRecIdTask { get; set; } = "No disponible";

        [JsonPropertyName("frmAssignmentId")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int frmAssignmentId { get; set; }

        [JsonPropertyName("frmParentNumber")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int frmParentNumber { get; set; }

        [JsonPropertyName("frmParentCategory")] 
        public string frmParentCategory { get; set; } = "No disponible";

        [JsonPropertyName("frmIdSitio")]
        public string frmIdSitio { get; set; } = "No disponible";

        [JsonPropertyName("frmCustId")]
        public string frmCustId { get; set; } = "No disponible";

        [JsonPropertyName("frmCodigoCierre")]
        public string frmCodigoCierre { get; set; } = "No disponible";

        [JsonPropertyName("frmParentOwner")]
        public string frmParentOwner { get; set; } = "No disponible";
    }

    #region Geolocation Classes
    public partial class Geolocation
    {
        public void Initialize()
        {
            geopoint ??= new Geopoint();
        }

        [JsonPropertyName("geohash")]
        public string geohash { get; set; } = "";

        [JsonPropertyName("geopoint")]
        public Geopoint geopoint { get; set; }
    }

    public partial class Geopoint
    {
        [JsonPropertyName("_latitude")]
        public double _latitude { get; set; } = 0;

        [JsonPropertyName("_longitude")]
        public double _longitude { get; set; } = 0;
    }
    #endregion

    #region ModulesConfig Classes
    public partial class ModulesConfig
    {
        public void Initialize()
        {
            users_autorized ??= new UsersAutorized();
        }

        [JsonPropertyName("active_out_range")]
        public bool active_out_range { get; set; }

        [JsonPropertyName("active_carret")]
        public bool active_carret { get; set; }

        [JsonPropertyName("active_transfer")]
        public bool active_transfer { get; set; }

        [JsonPropertyName("users_autorized")]
        public UsersAutorized users_autorized { get; set; }

        [JsonPropertyName("date_created")]
        public long date_created { get; set; }

        [JsonPropertyName("relationOneToMany")]
        public bool relationOneToMany { get; set; }

        [JsonPropertyName("active")]
        public bool active { get; set; }

        [JsonPropertyName("createElements")]
        public bool createElements { get; set; }

        [JsonPropertyName("reject_assignment")]
        public bool reject_assignment { get; set; }

        [JsonPropertyName("selectEquipment")]
        public bool selectEquipment { get; set; }

        [JsonPropertyName("collectionSearch")]
        public string collectionSearch { get; set; } = "No disponible";

        [JsonPropertyName("createOrders")]
        public bool createOrders { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; } = "No disponible";

        [JsonPropertyName("active_category")]
        public bool active_category { get; set; }

        [JsonPropertyName("active_autorize_events")]
        public bool active_autorize_events { get; set; }

        [JsonPropertyName("selectCustomer")]
        public bool selectCustomer { get; set; }

        [JsonPropertyName("order")]
        public string order { get; set; } = "No disponible";

        [JsonPropertyName("category")]
        public string category { get; set; } = "No disponible";

        [JsonPropertyName("forms")]
        public List<string> forms { get; set; } = new List<string>();
    }

    public partial class UsersAutorized
    {
        [JsonPropertyName("all")]
        public bool all { get; set; }

        [JsonPropertyName("user_uid")]
        public List<object> user_uid { get; set; } = new List<object>();
    }
    #endregion

    #region Element Classes
    public partial class Element
    {
        public void Initialize()
        {
                user ??= new User();
                info ??= new Info();
        }

        #region Variables simples
        [JsonPropertyName("title")]
        public string title { get; set; } = "No disponible";

        [JsonPropertyName("type")]
        public string type { get; set; } = "No disponible";

        [JsonPropertyName("url_map")]
        public string url_map { get; set; } = "No disponible";

        [JsonPropertyName("started_by")]
        public string started_by { get; set; } = "No disponible";

        [JsonPropertyName("last_modified_by")]
        public string last_modified_by { get; set; } = "No disponible";

        [JsonPropertyName("value")]
        public string? value { get; set; } = "No disponible";

        [JsonPropertyName("isComment")]
        public bool? isComment { get; set; } = false;

        [JsonPropertyName("isMap")]
        public bool? isMap { get; set; } = false;

        [JsonPropertyName("isPhotos")]
        public bool? isPhotos { get; set; } = false;

        [JsonPropertyName("isQuestionnaires")]
        public bool? isQuestionnaires { get; set; } = false;

        [JsonPropertyName("isSignature")]
        public bool? isSignature { get; set; } = false;
        #endregion

        #region Variables con Timestamp y DateTimeOffset
        [JsonPropertyName("started_at_utc")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp started_at_utc { get; set; }

        [JsonPropertyName("started_at_utc_dateTimeOffset")]
        public DateTimeOffset started_at_utc_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(started_at_utc._seconds)
                    .AddTicks(started_at_utc._nanoseconds / 100);
        }

        [JsonPropertyName("last_modified_at_utc")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp last_modified_at_utc { get; set; }

        [JsonPropertyName("last_modified_at_utc_dateTimeOffset")]
        public DateTimeOffset last_modified_at_utc_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(last_modified_at_utc._seconds)
                    .AddTicks(last_modified_at_utc._nanoseconds / 100);
        }
        #endregion

        #region Variables Complejas
        [JsonPropertyName("user")]
        public User user { get; set; } = new User();

        [JsonPropertyName("info")]
        public Info info { get; set; } //= new Info();

        [JsonPropertyName("items")]
        public List<Item> items { get; set; } = [];

        #endregion

    }

    public partial class User
    {
        [JsonPropertyName("email")]
        public string? email { get; set; } = "No disponible";

        [JsonPropertyName("name")]
        public string? name { get; set; } = "No disponible";

        [JsonPropertyName("photo_url")]
        public string? photo_url { get; set; } = "No disponible";

        [JsonPropertyName("uid")]
        public string? uid { get; set; } = "No disponible";
    }

    public partial class Info
    {
        [JsonPropertyName("address")]
        public string address { get; set; } = "No disponible";

        [JsonPropertyName("check_date")]
        public string check_date { get; set; } = "No disponible";

        [JsonPropertyName("check_date_utc")]
        [JsonConverter(typeof(TimestampOrEmptyConverter))]
        public Timestamp check_date_utc { get; set; }

        [JsonPropertyName("check_date_utc_dateTimeOffset")]
        public DateTimeOffset check_date_utc_dateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(check_date_utc._seconds)
                    .AddTicks(check_date_utc._nanoseconds / 100);
        }

        [JsonPropertyName("geolocation")]
        public Geolocation geolocation { get; set; } = new Geolocation();

        [JsonPropertyName("latitude")]
        public string latitude { get; set; } = "No disponible";

        [JsonPropertyName("longitude")]
        public string longitude { get; set; } = "No disponible";
    }

    public partial class Item
    {
        [JsonPropertyName("title")]
        public string title { get; set; } = "No disponible";

        [JsonPropertyName("sub_title")]
        public string sub_title { get; set; } = "No disponible";

        [JsonPropertyName("type")]
        public string type { get; set; } = "No disponible";

        [JsonPropertyName("thumbnail_url")]
        public string thumbnail_url { get; set; } = "No disponible";

        [JsonPropertyName("thumbnail")]
        public bool? thumbnail { get; set; } = false;

        [JsonPropertyName("isNormal")]
        public bool? isNormal { get; set; } = false;

        [JsonPropertyName("isCheck")]
        public bool? isCheck { get; set; } = false;

        [JsonPropertyName("isSignature")]
        public bool? isSignature { get; set; } = false;

        [JsonPropertyName("isQuestionnaires")]
        public bool? isQuestionnaires { get; set; } = false;

        [JsonPropertyName("value")]
        [JsonConverter(typeof(ItemValueJsonConverter))]
        public ItemValue? value { get; set; }
    }

    #endregion
}