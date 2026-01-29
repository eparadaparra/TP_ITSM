using System.Text.Json;
using System.Text.Json.Serialization;
using TP_ITSM.Models.Execon;

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

    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (int.TryParse(reader.GetString(), out int result))
                    return result;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            return 0; // o lanzar una excepción
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class TimestampOrEmptyConverter : JsonConverter<Timestamp>
    {
        public override Timestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (string.IsNullOrEmpty(str))
                    {
                        return new Timestamp();
                    }
                    // Opcional: intentar parsear de string si es una fecha
                    return ParseFromString(str);

                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<Timestamp>(ref reader, options);

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"Cannot convert {reader.TokenType} to Timestamp");
            }
        }

        public override void Write(Utf8JsonWriter writer, Timestamp value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

        private Timestamp ParseFromString(string str)
        {
            // Implementar lógica para convertir string a Timestamp si es necesario
            // Por ejemplo, si el string es una fecha ISO
            return new Timestamp();
        }
    }
}
