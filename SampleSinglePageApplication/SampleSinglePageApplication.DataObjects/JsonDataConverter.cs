using System;
using Newtonsoft.Json;

namespace SampleSinglePageApplication;

/// <summary>
/// object used to serialize DateTime objects and include the UTC timezone
/// </summary>
public class UTCDateTimeConverter : JsonConverter<DateTime?>
{
    private TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;// TimeZoneInfo.FindSystemTimeZoneById("UTC");

    /// <summary>
    /// override of built in property
    /// </summary>
    /// <param name="reader">the JsonReader</param>
    /// <param name="objectType">the object to check</param>
    /// <param name="existingValue">the existing value</param>
    /// <param name="serializer">the JsonSerializer</param>
    /// <returns>the passed DateTime object converted to TimeZoneInfo in UTC</returns>
    public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string rawDate = String.Empty;
        string dateType = "";

        if (reader.Value != null) {
            if (reader.Value.GetType() == typeof(DateTime)) {
                dateType = "datetime";
                rawDate = ((DateTime)reader.Value).ToString();
            } else if (reader.Value.GetType() == typeof(DateTime?)) {
                dateType = "datetime?";
                rawDate = ((DateTime)reader.Value).ToString();
            } else if (reader.Value.GetType() == typeof(DateTimeOffset)) {
                dateType = "datetimeoffset";
                rawDate = ((DateTimeOffset)reader.Value).ToString();
            } else if (reader.Value.GetType() == typeof(DateTimeOffset?)) {
                dateType = "datetimeoffset?";
                rawDate = ((DateTimeOffset)reader.Value).ToString();
            }
        } else {
            try {
                rawDate = (string)reader.Value;
            } catch { }
        }


        if (String.IsNullOrEmpty(rawDate)) {
            return null;
        } else {
            try {
                if (dateType == "datetime?") {
                    var output = ((DateTime?)Convert.ToDateTime(rawDate).ToUniversalTime());
                    return output;
                } else {
                    var output = Convert.ToDateTime(rawDate).ToUniversalTime();
                    return output;
                }
            } catch (Exception ex) {
                // It's not a date after all, so just return the default value
                if (objectType == typeof(DateTime?)) {
                    return null;
                }

                return null;
            }
        }
    }

    /// <summary>
    /// override of built in property
    /// </summary>
    /// <param name="writer">the JsonWriter</param>
    /// <param name="value">the existing value</param>
    /// <param name="serializer">the JsonSerializer</param>
    public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
    {
        if (value != null) {
            //string tzName = "Pacific Standard Time";
            //var output = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((DateTime)value, tzName);
            //writer.WriteValue(output);

            DateTime v = (DateTime)value;

            if (v.Kind == DateTimeKind.Local) {
                v = v.ToUniversalTime();
            }

            var output = TimeZoneInfo.ConvertTimeFromUtc(v, utcTimeZone);
            writer.WriteValue(output);
        }
    }
}