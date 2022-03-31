using System;
using Newtonsoft.Json;

namespace SampleSinglePageApplication;

/// <summary>
/// object used to serialize DateTime objects and include the UTC timezone
/// </summary>
public class UTCDateTimeConverter : JsonConverter
{
    private TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;// TimeZoneInfo.FindSystemTimeZoneById("UTC");

    /// <summary>
    /// override of built in property
    /// </summary>
    /// <param name="objectType">the object being checked</param>
    /// <returns>true if the object is a DateTime object</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime);
    }

    /// <summary>
    /// override of built in property
    /// </summary>
    /// <param name="reader">the JsonReader</param>
    /// <param name="objectType">the object to check</param>
    /// <param name="existingValue">the existing value</param>
    /// <param name="serializer">the JsonSerializer</param>
    /// <returns>the passed DateTime object converted to TimeZoneInfo in UTC</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null) {
            return null;
        }

        if (String.IsNullOrWhiteSpace(reader.Value.ToString())) {
            return null;
        }

        if (reader.Value != null) {
            var localTime = (DateTime.Parse((string)reader.Value)).ToLocalTime();

            string tzName = "Pacific Standard Time";

            var output = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(localTime, tzName);
            return output;
        } else {
            return null;
        }
    }

    /// <summary>
    /// override of built in property
    /// </summary>
    /// <param name="writer">the JsonWriter</param>
    /// <param name="value">the existing value</param>
    /// <param name="serializer">the JsonSerializer</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value != null) {
            string tzName = "Pacific Standard Time";
            var output = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((DateTime)value, tzName);
            writer.WriteValue(output);
        }
    }
}