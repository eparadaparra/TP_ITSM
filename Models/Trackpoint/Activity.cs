using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Trackpoint
{
    public class FirebaseId
    {
        [JsonPropertyName("firebaseId")]
        public string firebaseId { get; set; }
    }

    public class ActivityReq
    {
        [JsonPropertyName("EX_IdActividadTP")]
        public string? scheduled_type_event { get; set; }

        [JsonPropertyName("NombreEvento")]
        public string? scheduled_name_event { get; set; }

        [JsonPropertyName("client_uuid")]
        public string? scheduled_client_uuid { get; set; }

        public string? scheduled_periodicity { get; set; } = "notrepeat";

        public string? id_user { get; set; } //= 'kPOhsrRGKiNDeB6ntf8kPPqMmwE3';
        
        [Required]
        [JsonPropertyName("EX_Latitud")]
        public double latitude { get; set; } 
        
        [Required]
        [JsonPropertyName("EX_Longitud")]
        public double longitude { get; set; } 

        [JsonPropertyName("EX_Direccion")]
        public string? scheduled_address { get; set; }

        [JsonPropertyName("scheduled_date_programming")]
        public string? scheduled_date_programming { get; set; }

        [JsonPropertyName("scheduled_hour_since")]
        public string? scheduled_hour_since { get; set; }


        [JsonPropertyName("scheduled_hour_limit")]
        public string? scheduled_hour_limit { get; set; }

        [Required]
        public int? scheduled_expiration_date { get; set; }

        [JsonPropertyName("Details")]
        public string? scheduled_instructions { get; set; }

        [JsonPropertyName("EX_Zona")]
        public string? scheduled_clasification_name { get; set; }
        
        public string? scheduled_clasification { get; set; } = "";

        [JsonPropertyName("EX_PlazaCobertura")]
        public string? scheduled_subclasification_name { get; set; }

        public string? scheduled_subclasification { get; set; } = "";
    }
    public class Preload
    {
        [JsonPropertyName("RecId")]
        public string? frmRecIdTask { get; set; }
        
        [JsonPropertyName("AssignmentID")]
        public long frmAssignmentId { get; set; }

        [JsonPropertyName("ParentNumber")]
        public long frmParentNumber { get; set; }

        [JsonPropertyName("ParentLink_Category")]
        public string? frmParentCategory { get; set; }

        [JsonPropertyName("EX_IdSitio")]
        public string? frmIdSitio { get; set; }

        [JsonPropertyName("CustID")]
        public string? frmCustId { get; set; }

        [JsonPropertyName("EX_CodigoCierre")]
        public string? frmCodigoCierre { get; set; }

        [JsonPropertyName("DisplayName")]
        public string? frmParentOwner { get; set; }
    }
}
