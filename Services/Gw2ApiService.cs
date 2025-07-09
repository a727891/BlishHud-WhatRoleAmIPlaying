using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Soeed.WhatAmIPlaying.Models;

namespace Soeed.WhatAmIPlaying.Services
{
    public class Gw2ApiService
    {
        private static readonly Logger Logger = Logger.GetLogger<Gw2ApiService>();
        private readonly Gw2ApiManager _apiManager;

        public Gw2ApiService(Gw2ApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        /// <summary>
        /// Gets all characters for the current user
        /// </summary>
        public async Task<List<Character>> GetUserCharactersAsync()
        {
            try
            {
                // For now, skip permission check to avoid compilation issues
                var characters = await _apiManager.Gw2ApiClient.V2.Characters.AllAsync();
                return characters.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get user characters");
                return new List<Character>();
            }
        }

        /// <summary>
        /// For now, returns all available roles since we can't easily determine unlocked specs
        /// In a future version, we'll implement proper specialization checking
        /// </summary>
        public Task<HashSet<string>> GetUnlockedEliteSpecsAsync()
        {
            // For now, return all elite specs as "unlocked" since we can't easily check
            // This will be improved in future versions
            var allSpecs = new HashSet<string>
            {
                "Dragonhunter", "Firebrand", "Willbender",
                "Berserker", "Spellbreaker", "Bladesworn",
                "Scrapper", "Holosmith", "Mechanist",
                "Druid", "Soulbeast", "Untamed",
                "Daredevil", "Deadeye", "Specter",
                "Tempest", "Weaver", "Catalyst",
                "Chronomancer", "Mirage", "Virtuoso",
                "Reaper", "Scourge", "Harbinger",
                "Herald", "Renegade", "Vindicator"
            };

            Logger.Info("Using all elite specs as unlocked (development mode)");
            return Task.FromResult(allSpecs);
        }

        /// <summary>
        /// Checks if the user has any characters with unlocked elite specs
        /// </summary>
        public async Task<bool> HasUnlockedEliteSpecsAsync()
        {
            var unlockedSpecs = await GetUnlockedEliteSpecsAsync();
            return unlockedSpecs.Count > 0;
        }

        /// <summary>
        /// Gets a list of available roles filtered by unlocked elite specs
        /// </summary>
        public async Task<List<RoleSuggestion>> GetAvailableRolesAsync(RoleType roleType)
        {
            var unlockedSpecs = await GetUnlockedEliteSpecsAsync();
            var allRoles = Soeed.WhatAmIPlaying.WhatAmIPlayingModule.RoleConfig.GetAllRoles(roleType);
            
            // Filter roles to only include unlocked elite specs
            var availableRoles = allRoles.Where(role => unlockedSpecs.Contains(role.EliteSpec)).ToList();
            
            Logger.Info($"Found {availableRoles.Count} available roles for {roleType} (from {allRoles.Count} total roles)");
            
            return availableRoles;
        }

        /// <summary>
        /// Gets character count for the user
        /// </summary>
        public async Task<int> GetCharacterCountAsync()
        {
            try
            {
                var characters = await GetUserCharactersAsync();
                return characters.Count;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get character count");
                return 0;
            }
        }
    }
} 