using Avalonia.Media;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.Converters;
public class JsonConverter_SolidColorBrush : JsonConverter<SolidColorBrush> {

    public override SolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var hexValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(hexValue) is false) {
                return new SolidColorBrush(Core.Colors.ParseHexToColor(hexValue));
            }
            else {
                return new SolidColorBrush(Avalonia.Media.Colors.Purple);
            }
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, SolidColorBrush? value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteStringValue("#800080");
            return;
        }
        writer.WriteStringValue(Core.Colors.ToHex(value.Color));
    }
}
