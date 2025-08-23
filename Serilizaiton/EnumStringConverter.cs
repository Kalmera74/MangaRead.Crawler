using System.Text.Json;
using System.Text.Json.Serialization;

public class EnumStringConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string enumString = reader.GetString() ?? throw new JsonException();
        return Enum.Parse<TEnum>(enumString, true);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}