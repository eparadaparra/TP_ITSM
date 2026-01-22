using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP_ITSM.Models
{
    [JsonConverter(typeof(ItemValueJsonConverter))]
    public class ItemValue 
    {
        [JsonIgnore] public bool? Bool { get; set; }
        [JsonIgnore] public string? String { get; set; }
        [JsonIgnore] public int? Int { get; set; }

        public static implicit operator ItemValue (bool b)    
            => new ItemValue  { Bool = b };
        public static implicit operator ItemValue (string s)  
            => new ItemValue  { String = s };
        public static implicit operator ItemValue (int i)     
            => new ItemValue  { Int = i };

        public override string? ToString()
        {
            if (Bool.HasValue) return Bool.Value.ToString();
            if (Int.HasValue)  return Int.Value.ToString();
            return String;
        }
    }

    public sealed class ItemValueJsonConverter : JsonConverter<ItemValue >
    {
        public override ItemValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null   => null,
                JsonTokenType.True   => (ItemValue)true,
                JsonTokenType.False  => (ItemValue)false,
                JsonTokenType.String => (ItemValue)(reader.GetString() ?? ""),
                JsonTokenType.Number => (ItemValue)reader.GetInt32(),
                _ => throw new JsonException($"Token no soportado para Value: {reader.TokenType}"),
            };
        }
        public override void Write(Utf8JsonWriter writer, ItemValue  value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }
            if (value.String is not null) { writer.WriteStringValue(value.String); return; }
            if (value.Bool.HasValue) { writer.WriteBooleanValue(value.Bool.Value); return; }
            if (value.Int.HasValue) { writer.WriteNumberValue(value.Int.Value); return; }
            
            writer.WriteNullValue();
        }
    }

}
