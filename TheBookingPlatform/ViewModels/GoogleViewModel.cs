using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookingPlatform.ViewModels
{
    public class GoogleCalendarCreateModel
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("conferenceProperties")]
        public ConferenceProperties ConferenceProperties { get; set; }
    }

    public class ConferenceProperties
    {
        [JsonProperty("allowedConferenceSolutionTypes")]
        public List<string> AllowedConferenceSolutionTypes { get; set; }
    }
    public class CalendarList
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("nextSyncToken")]
        public string NextSyncToken { get; set; }

        [JsonProperty("items")]
        public List<CalendarListEntry> Items { get; set; }
    }

    public class CalendarListEntry
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("colorId")]
        public string ColorId { get; set; }

        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty("foregroundColor")]
        public string ForegroundColor { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("accessRole")]
        public string AccessRole { get; set; }

        [JsonProperty("defaultReminders")]
        public List<Reminder> DefaultReminders { get; set; }

        [JsonProperty("notificationSettings")]
        public NotificationSettings NotificationSettings { get; set; }

        [JsonProperty("conferenceProperties")]
        public ConferenceProperties ConferenceProperties { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }
    }

    public class Reminder
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("minutes")]
        public int Minutes { get; set; }
    }

    public class NotificationSettings
    {
        [JsonProperty("notifications")]
        public List<Notification> Notifications { get; set; }
    }

    public class Notification
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }



    public class TokenResponseForDB
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
    public class MultiCalendarModel
    {
        public int employeeID { get; set; }
        public string summary { get; set; }
        public string description { get; set; }

    }
}