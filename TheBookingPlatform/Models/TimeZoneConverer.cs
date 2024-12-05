using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using NodaTime;
using NodaTime.TimeZones;
namespace TheBookingPlatform.Models
{
   

    public class TimeZoneConverter
    {
        /// <summary>
        /// Converts a given DateTime from the system's local timezone to a target timezone.
        /// </summary>
        /// <param name="dateTime">The DateTime to be converted.</param>
        /// <param name="targetTimeZoneId">The target timezone ID (IANA format, e.g., "Europe/Amsterdam").</param>
        /// <returns>The converted DateTime in the target timezone.</returns>
        public static DateTime ConvertToTimeZone(DateTime dateTime, string targetTimeZoneId)
        {
            try
            {
                // Get the target timezone
                DateTimeZone targetTimeZone = DateTimeZoneProviders.Tzdb[targetTimeZoneId];

                // Convert DateTime to NodaTime's LocalDateTime
                LocalDateTime localDateTime = LocalDateTime.FromDateTime(dateTime);

                // Get the system's default timezone
                DateTimeZone systemTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

                // Convert to ZonedDateTime in the system's timezone
                ZonedDateTime systemZonedDateTime = systemTimeZone.AtStrictly(localDateTime);

                // Convert to the target timezone
                ZonedDateTime targetZonedDateTime = systemZonedDateTime.WithZone(targetTimeZone);

                // Convert back to DateTime and return
                return targetZonedDateTime.ToDateTimeUnspecified();
            }
            catch (DateTimeZoneNotFoundException)
            {
                throw new ArgumentException($"The timezone ID '{targetTimeZoneId}' was not found.", nameof(targetTimeZoneId));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during timezone conversion.", ex);
            }
        }
    }

}