using Blish_HUD.Settings;
using Soeed.WhatAmIPlaying.Models;

namespace Soeed.WhatAmIPlaying.Services
{
    public class SettingService
    {
        public SettingService(SettingCollection settings)
        {
            // Define module settings here
            // ShowNotifications = settings.DefineSetting(
            //     "show_notifications",
            //     true,
            //     () => "Show Notifications",
            //     () => "Show notifications when getting random role suggestions");

            // AutoRefreshRoles = settings.DefineSetting(
            //     "auto_refresh_roles",
            //     false,
            //     () => "Auto Refresh Roles",
            //     () => "Automatically refresh role configuration on module load");

            // DefaultRoleType = settings.DefineSetting(
            //     "default_role_type",
            //     RoleType.FullRandom,
            //     () => "Default Role Type",
            //     () => "Default role type to use when clicking the corner icon");
        }

        // public SettingEntry<bool> ShowNotifications { get; }
        // public SettingEntry<bool> AutoRefreshRoles { get; }
        // public SettingEntry<RoleType> DefaultRoleType { get; }
    }
} 