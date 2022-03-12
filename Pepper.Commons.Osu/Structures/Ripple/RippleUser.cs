using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API.Ripple
{
    /// <summary>
    /// Representing a Ripple user.
    /// See <a href="https://docs.ripple.moe/docs/api/types#user">https://docs.ripple.moe/docs/api/types#user</a>.
    /// </summary>
    internal class RippleUser
    {
        [JsonProperty("id")] public int UserId { get; set; }
        [JsonProperty("username")] public string Username { get; set; } = "";
        [JsonProperty("username_aka")] public string UsernameAlsoKnownAs { get; set; } = "";
        [JsonProperty("registered_on")] public DateTimeOffset RegisteredOn { get; set; }
        [JsonProperty("privileges")] public ulong Privileges { get; set; }
        [JsonProperty("latest_activity")] public DateTimeOffset LastActive { get; set; }
        [JsonProperty("country")] private string CountryCode { get; set; } = "";
        public RegionInfo Country => new(CountryCode);
        public Dictionary<GameMode, RippleUserStatistics> Statistics = new();

        [JsonProperty("std")]
        private RippleUserStatistics StandardStatistics
        {
            set => Statistics[GameMode.Standard] = value;
        }

        [JsonProperty("ctb")]
        private RippleUserStatistics CatchTheBeatStatistics
        {
            set => Statistics[GameMode.Catch] = value;
        }

        [JsonProperty("mania")]
        private RippleUserStatistics ManiaStatistics
        {
            set => Statistics[GameMode.Mania] = value;
        }

        [JsonProperty("taiko")]
        private RippleUserStatistics TaikoStatistics
        {
            set => Statistics[GameMode.Taiko] = value;
        }
    }
}