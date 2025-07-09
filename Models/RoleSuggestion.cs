using System;
using Newtonsoft.Json;

namespace Soeed.WhatAmIPlaying.Models
{
    [Serializable]
    public class RoleSuggestion
    {
        [JsonProperty("profession")]
        public string Profession { get; set; } = string.Empty;

        [JsonProperty("elite_spec")]
        public string EliteSpec { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("role_type")]
        public RoleType RoleType { get; set; }

        [JsonProperty("provides_quickness")]
        public bool ProvidesQuickness { get; set; }

        [JsonProperty("provides_alacrity")]
        public bool ProvidesAlacrity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("build_url")]
        public string BuildUrl { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Profession} - {EliteSpec} ({Role})";
        }
    }
} 