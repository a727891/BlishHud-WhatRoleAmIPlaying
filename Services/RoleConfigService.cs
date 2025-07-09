using Blish_HUD;
using Soeed.WhatRoleAmIPlaying.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soeed.WhatRoleAmIPlaying.Services
{
    public class RoleConfigService
    {
        private readonly RoleConfig _config;
        private readonly Random _random = new();

        public RoleConfigService(RoleConfig config)
        {
            _config = config;
        }

        public RoleSuggestion? GetRandomRole(RoleType roleType)
        {
            var availableRoles = GetAvailableRoles(roleType);
            
            if (!availableRoles.Any())
            {
                Logger.GetLogger<Soeed.WhatRoleAmIPlaying.WhatRoleAmIPlayingModule>().Warn($"No roles available for type: {roleType}");
                return null;
            }

            var randomIndex = _random.Next(availableRoles.Count);
            var selectedRole = availableRoles[randomIndex];
            
            Logger.GetLogger<Soeed.WhatRoleAmIPlaying.WhatRoleAmIPlayingModule>().Info($"Selected role: {selectedRole}");
            return selectedRole;
        }

        private List<RoleSuggestion> GetAvailableRoles(RoleType roleType)
        {
            if (roleType == RoleType.FullRandom)
            {
                return _config.Roles.ToList();
            }

            return roleType switch
            {
                RoleType.DPS => _config.Roles
                    .Where(r => r.RoleType == RoleType.DPS && 
                               !r.ProvidesQuickness && !r.ProvidesAlacrity)
                    .ToList(),
                RoleType.PowerDPS => _config.Roles
                    .Where(r => r.RoleType == RoleType.DPS && 
                               string.Equals(r.Role, "Power", StringComparison.OrdinalIgnoreCase) &&
                               !r.ProvidesQuickness && !r.ProvidesAlacrity)
                    .ToList(),
                RoleType.ConditionDPS => _config.Roles
                    .Where(r => r.RoleType == RoleType.DPS && 
                               string.Equals(r.Role, "Condition", StringComparison.OrdinalIgnoreCase) &&
                               !r.ProvidesQuickness && !r.ProvidesAlacrity)
                    .ToList(),
                RoleType.Healer => _config.Roles
                    .Where(r => r.RoleType == RoleType.Healer)
                    .ToList(),
                _ => _config.Roles
                    .Where(r => r.RoleType == roleType)
                    .ToList()
            };
        }

        public List<RoleSuggestion> GetRolesByBoon(bool providesQuickness, bool providesAlacrity)
        {
            return _config.Roles
                .Where(r => r.ProvidesQuickness == providesQuickness && r.ProvidesAlacrity == providesAlacrity)
                .ToList();
        }

        public List<RoleSuggestion> GetDPSWithBoon(bool providesQuickness, bool providesAlacrity)
        {
            return _config.Roles
                .Where(r => r.RoleType == RoleType.DPS && 
                           r.ProvidesQuickness == providesQuickness && 
                           r.ProvidesAlacrity == providesAlacrity)
                .ToList();
        }

        public List<RoleSuggestion> GetHealerWithBoon(bool providesQuickness, bool providesAlacrity)
        {
            return _config.Roles
                .Where(r => r.RoleType == RoleType.Healer && 
                           r.ProvidesQuickness == providesQuickness && 
                           r.ProvidesAlacrity == providesAlacrity)
                .ToList();
        }

        public List<RoleSuggestion> GetRolesByType(RoleType roleType)
        {
            return GetAvailableRoles(roleType);
        }

        public List<RoleSuggestion> GetRolesByProfession(string profession)
        {
            return _config.Roles
                .Where(r => string.Equals(r.Profession, profession, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<RoleSuggestion> GetRolesByEliteSpec(string eliteSpec)
        {
            return _config.Roles
                .Where(r => string.Equals(r.EliteSpec, eliteSpec, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public int GetTotalRoleCount()
        {
            return _config.Roles.Count;
        }

        public int GetRoleCountByType(RoleType roleType)
        {
            return GetAvailableRoles(roleType).Count;
        }

        public List<RoleSuggestion> GetAllRoles(RoleType roleType)
        {
            return GetAvailableRoles(roleType);
        }

        public ProfessionInfo? GetProfessionByName(string professionName)
        {
            return _config.Professions
                .FirstOrDefault(p => string.Equals(p.Name, professionName, StringComparison.OrdinalIgnoreCase));
        }

        public EliteSpecInfo? GetEliteSpecByName(string eliteSpecName)
        {
            return _config.EliteSpecs
                .FirstOrDefault(e => string.Equals(e.Name, eliteSpecName, StringComparison.OrdinalIgnoreCase));
        }
    }
} 