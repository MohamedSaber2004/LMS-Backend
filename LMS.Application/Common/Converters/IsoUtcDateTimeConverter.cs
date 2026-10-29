using System.Text.Json;
using System.Text.Json.Serialization;
using LMS.Application.Common.Helpers;

namespace LMS.Application.Common.Converters
{
    public class IsoUtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var date))
            {
                return DateTimeHelper.EnsureUtc(date);
            }
            return DateTimeHelper.EnsureUtc(reader.GetDateTime());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(DateTimeHelper.FormatUtc(value));
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
                return DateTimeHelper.EnsureUtc(date);
            }
            return DateTimeHelper.EnsureUtc(reader.GetDateTime());
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(DateTimeHelper.FormatUtc(value.Value));
        }
    }
}
