using CommandTimer.Core.Static;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.Converters;

public class JsonConverter_AppColor : JsonConverter<AppColor> {

    public override AppColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var hexValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(hexValue) is false) {
                return ColorUtilities.ParseHexToColor(hexValue);
            }
            else {
                return AppColor.Transparent;
            }
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, AppColor value, JsonSerializerOptions options) {
        writer.WriteStringValue(ColorUtilities.ToHex(value));
    }
}