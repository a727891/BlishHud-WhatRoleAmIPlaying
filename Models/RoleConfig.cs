using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Soeed.WhatAmIPlaying.Models
{
    [Serializable]
    public class RoleConfig
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "0.1.0";

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; } = string.Empty;

        [JsonProperty("roles")]
        public List<RoleSuggestion> Roles { get; set; } = new();

        [JsonProperty("professions")]
        public List<ProfessionInfo> Professions { get; set; } = new();

        [JsonProperty("elite_specs")]
        public List<EliteSpecInfo> EliteSpecs { get; set; } = new();
    }

    [Serializable]
    public class ProfessionInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    [Serializable]
    public class EliteSpecInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("profession")]
        public string Profession { get; set; } = string.Empty;

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonProperty("background")]
        public string Background { get; set; } = string.Empty;
    }
} 