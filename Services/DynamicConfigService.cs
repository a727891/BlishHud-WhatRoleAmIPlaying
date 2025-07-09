using Blish_HUD;
using Newtonsoft.Json;
using Soeed.WhatRoleAmIPlaying.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soeed.WhatRoleAmIPlaying.Services
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
                Logger.GetLogger<WhatRoleAmIPlayingModule>().Warn(ex, $"Failed to fetch config from {url}");
                return default;
            }
        }

        public async Task<RoleConfig?> LoadConfig()
        {
#if DEBUG
            // var path = $"{WhatRoleAmIPlayingModule.STATIC_HOST_URL}/roles.json";
            var path = $"http://localhost:3000/roles.json";
#else
            var path = $"{WhatRoleAmIPlayingModule.STATIC_HOST_URL}/roles.json";
#endif
            Logger.GetLogger<WhatRoleAmIPlayingModule>().Info($"Loading role config from {path}");
            var config = await Fetch<RoleConfig>(path);
            
            if (config == null)
            {
                Logger.GetLogger<WhatRoleAmIPlayingModule>().Error("Failed to load role configuration");
                return new RoleConfig();
            }

            Logger.GetLogger<WhatRoleAmIPlayingModule>().Info($"Loaded {config.Roles.Count} roles from configuration");
            return config;
        }

        public void Dispose()
        {
            // HttpClient is static, so we don't dispose it here
        }
    }
} 