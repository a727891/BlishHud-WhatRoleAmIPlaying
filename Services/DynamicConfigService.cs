using Blish_HUD;
using Newtonsoft.Json;
using Soeed.WhatAmIPlaying.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soeed.WhatAmIPlaying.Services
{
    public class DynamicConfigService : IDisposable
    {
        private static readonly HttpClient HttpClient = new();

        public DynamicConfigService()
        {
        }

        protected async Task<T?> Fetch<T>(string url) where T : class
        {
            try
            {
                var response = await HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                Logger.GetLogger<WhatAmIPlayingModule>().Warn(ex, $"Failed to fetch config from {url}");
                return default;
            }
        }

        public async Task<RoleConfig?> LoadConfig()
        {
#if DEBUG
            // var path = $"{WhatAmIPlayingModule.STATIC_HOST_URL}/roles.json";
            var path = $"http://localhost:3000/roles.json";
#else
            var path = $"{WhatAmIPlayingModule.STATIC_HOST_URL}/roles.json";
#endif
            Logger.GetLogger<WhatAmIPlayingModule>().Info($"Loading role config from {path}");
            var config = await Fetch<RoleConfig>(path);
            
            if (config == null)
            {
                Logger.GetLogger<WhatAmIPlayingModule>().Error("Failed to load role configuration");
                return new RoleConfig();
            }

            Logger.GetLogger<WhatAmIPlayingModule>().Info($"Loaded {config.Roles.Count} roles from configuration");
            return config;
        }

        public void Dispose()
        {
            // HttpClient is static, so we don't dispose it here
        }
    }
} 