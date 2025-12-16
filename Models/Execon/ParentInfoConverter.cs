using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TP_ITSM.Models.Execon
{
    public class ParentInfoNewtonsoftConverter : JsonConverter<ParentInfo>
    {
        private static readonly string[] PossibleParentNumberFields =
        {
        "IncidentNumber",
        "ChangeNumber",
        "ServiceReqNumber",
        "ReleaseNumber",
        "ProblemNumber",
        "ParentNumber"
    };

        public override ParentInfo ReadJson(
            JsonReader reader,
            Type objectType,
            ParentInfo? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            var result = new ParentInfo
            {
                //RecId = (string?)obj["RecId"],
                Owner_Valid = (string?)obj["Owner_Valid"],
                Owner = (string?)obj["Owner"],
                Subject = (string?)obj["Subject"],
                EX_LocationID_Link_RecID = (string?)obj["EX_LocationID_Link_RecID"],
                EX_CustID_Link_RecID = (string?)obj["EX_CustID_Link_RecID"],
                CreatedDateTime = (DateTimeOffset?)obj["CreatedDateTime"] ?? default
            };

            // ParentNumber dinámico
            foreach (var field in PossibleParentNumberFields)
            {
                if (obj.TryGetValue(field, out JToken? token) &&
                    token.Type != JTokenType.Null)
                {
                    result.ParentNumber = token.Value<long>();
                    break;
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, ParentInfo? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            //writer.WritePropertyName("RecId");
            //writer.WriteValue(value?.RecId);

            writer.WritePropertyName("Owner_Valid");
            writer.WriteValue(value?.Owner_Valid);

            writer.WritePropertyName("Owner");
            writer.WriteValue(value?.Owner);

            writer.WritePropertyName("ParentNumber");
            writer.WriteValue(value?.ParentNumber);

            writer.WritePropertyName("Subject");
            writer.WriteValue(value?.Subject);

            writer.WritePropertyName("EX_LocationID_Link_RecID");
            writer.WriteValue(value?.EX_LocationID_Link_RecID);

            writer.WritePropertyName("EX_CustID_Link_RecID");
            writer.WriteValue(value?.EX_CustID_Link_RecID);

            writer.WritePropertyName("CreatedDateTime");
            writer.WriteValue(value?.CreatedDateTime);

            writer.WriteEndObject();
        }
    }
}