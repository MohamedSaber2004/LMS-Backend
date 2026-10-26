using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LMS.Application.Common.Converters
{
    public class IsoUtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var date))
            {
                return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date.ToUniversalTime(), DateTimeKind.Utc);
            }
            return DateTime.SpecifyKind(reader.GetDateTime().ToUniversalTime(), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utc = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc);
            writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        }
    }


    public class IsoUtcNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var date))
            {
                return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date.ToUniversalTime(), DateTimeKind.Utc);
            }
            return DateTime.SpecifyKind(reader.GetDateTime().ToUniversalTime(), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            var utc = value.Value.Kind == DateTimeKind.Utc ? value.Value : DateTime.SpecifyKind(value.Value.ToUniversalTime(), DateTimeKind.Utc);
            writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        }
    }
}
