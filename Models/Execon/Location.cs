using System.Text.Json.Serialization;

namespace TP_ITSM.Models.Execon
{
    public class LocationInfo
    {
        //[JsonPropertyName("RecId")]
        //public string RecId { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Address")]
        public string Address { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("Zip")]
        public string Zip { get; set; }

        [JsonPropertyName("EX_Colonia")]
        public string EX_Colonia { get; set; }

        [JsonPropertyName("EX_IdSitio")]
        public string EX_IdSitio { get; set; }

        [JsonPropertyName("EX_Zona")]
        public string EX_Zona { get; set; }

        [JsonPropertyName("EX_PlazaCobertura")]
        public string EX_PlazaCobertura { get; set; }

        [JsonPropertyName("EX_Latitud")]
        public double EX_Latitud { get; set; } = 0;

        [JsonPropertyName("EX_Longitud")]
        public double EX_Longitud { get; set; } = 0;

        [JsonPropertyName("EX_Direccion")]
        public string EX_Direccion
        {
            get
            {
                return string.Concat(Address, " ", EX_Colonia, " ", City, " CP: ", Zip ?? "00000", ", ", State);
            }
            set { } // Setter necesario para deserialización, pero puede ser vacío
        }
    }
}
