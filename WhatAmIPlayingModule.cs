using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Soeed.WhatAmIPlaying.Services;
using Soeed.WhatAmIPlaying.Models;

namespace Soeed.WhatAmIPlaying
{
    [Export(typeof(Module))]
    public class WhatAmIPlayingModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<WhatAmIPlayingModule>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        // Module constants
        public static string MODULE_VERSION = "0.1.0";
        public static string DIRECTORY_PATH = "whatamiplaying-data";
        public static string STATIC_HOST_URL = "https://bhm.blishhud.com/WhatAmIPlayingModule";

        // Services
        public static RoleConfigService RoleConfig { get; set; } = null!;
        public static SettingService Settings { get; set; } = null!;
        public static Gw2ApiService Gw2Api { get; set; } = null!;

        // UI Components
        private CornerIcon _cornerIcon = null!;
        private ContextMenuStrip _contextMenuStrip = null!;
        private StandardWindow _mainWindow = null!;
        private Panel _roleDisplayPanel = null!;
        private Image _professionIcon = null!;
        private Image _eliteSpecIcon = null!;
        private Image _boonIcon = null!;
        private Label _roleNameLabel = null!;
        private Label _roleDescriptionLabel = null!;
        private StandardButton _buildUrlButton = null!;
        private string _currentBuildUrl = string.Empty;

        [ImportingConstructor]
        public WhatAmIPlayingModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            WhatAmIPlayingModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            Settings = new SettingService(settings);
        }

        protected override async Task LoadAsync()
        {
            // Load dynamic configuration from static hosting
            var configLoader = new DynamicConfigService();
            var config = await configLoader.LoadConfig();
            
            if (config == null)
            {
                Logger.Error("Failed to load dynamic configuration");
                return;
            }

            RoleConfig = new RoleConfigService(config);
            Gw2Api = new Gw2ApiService(Gw2ApiManager);

            // Create UI components
            CreateCornerIcon();
            CreateMainWindow();

            Logger.Info("What Am I Playing module loaded successfully");
        }

        protected override void Update(GameTime gameTime)
        {
            // Update logic here
        }

        protected override void Unload()
        {
            _cornerIcon?.Dispose();
            _contextMenuStrip?.Dispose();
            _mainWindow?.Dispose();

            // Clear static members to prevent memory leaks
            WhatAmIPlayingModuleInstance = null;
        }

        private void CreateCornerIcon()
        {
            _cornerIcon = new CornerIcon
            {
                Icon = AsyncTexture2D.FromAssetId(156720), // Default icon
                BasicTooltipText = "What Am I Playing?",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            _contextMenuStrip = new ContextMenuStrip();
            
            // Add Full Random option
            _contextMenuStrip.AddMenuItem("Full Random").Click += (s, e) => GetRandomRole(RoleType.FullRandom);
            
            // Add separator
            _contextMenuStrip.AddMenuItem("-");
            
            // Add DPS options
            _contextMenuStrip.AddMenuItem("DPS - Any").Click += (s, e) => GetRandomRole(RoleType.DPS);
            _contextMenuStrip.AddMenuItem("DPS - Quickness").Click += (s, e) => GetRandomDPSWithBoon(true, false);
            _contextMenuStrip.AddMenuItem("DPS - Alacrity").Click += (s, e) => GetRandomDPSWithBoon(false, true);
            _contextMenuStrip.AddMenuItem("DPS - Both Boons").Click += (s, e) => GetRandomDPSWithBoon(true, true);
            
            // Add Healer options
            _contextMenuStrip.AddMenuItem("Healer - Any").Click += (s, e) => GetRandomRole(RoleType.Healer);
            _contextMenuStrip.AddMenuItem("Healer - Quickness").Click += (s, e) => GetRandomHealerWithBoon(true, false);
            _contextMenuStrip.AddMenuItem("Healer - Alacrity").Click += (s, e) => GetRandomHealerWithBoon(false, true);
            _contextMenuStrip.AddMenuItem("Healer - Both Boons").Click += (s, e) => GetRandomHealerWithBoon(true, true);
            
            
            
            _cornerIcon.Menu = _contextMenuStrip;
            _cornerIcon.Click += (s, e) => _mainWindow?.ToggleWindow();
        }

        private void CreateMainWindow()
        {

            AsyncTexture2D Background = AsyncTexture2D.FromAssetId(155985);
            Rectangle SettingPanelRegion = new(40, 26, 913, 691);
            Rectangle SettingPanelContentRegion = new(40, 26, 913, 691);
            Point SettingPanelWindowSize = new(600, 600);
            _mainWindow = new StandardWindow(Background, SettingPanelRegion, SettingPanelContentRegion, SettingPanelWindowSize)
            {
                Emblem = AsyncTexture2D.FromAssetId(155985),
                Title = "What Am I Playing?",
                Subtitle = "Get random role suggestions",
                Parent = GameService.Graphics.SpriteScreen,
                SavesPosition = true,
                Id = $"{nameof(WhatAmIPlayingModule)}_Main_123456"
            };

            // Create main container
            var mainContainer = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 10),
                Parent = _mainWindow,
                Size = new Point(_mainWindow.ContentRegion.Width, _mainWindow.ContentRegion.Height),
                CanScroll = true
            };

            // Add role selection buttons as a grid
            var randomButton = new StandardButton
            {
                Text = "Random",
                Parent = mainContainer,
                Size = new Point(400, 40),
                Location = new Point((_mainWindow.ContentRegion.Width - 400) / 2, 10)
            };
            randomButton.Click += (s, e) => GetRandomRole(RoleType.FullRandom);

            var gridPanel = new Panel
            {
                Parent = mainContainer,
                Size = new Point(420, 150),
                Location = new Point((_mainWindow.ContentRegion.Width - 420) / 2, 60)
            };

            // Column 1
            var dpsButton = new StandardButton
            {
                Text = "DPS",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(0, 0)
            };
            dpsButton.Click += (s, e) => GetRandomRole(RoleType.DPS);

            var powerButton = new StandardButton
            {
                Text = "Power",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(0, 40)
            };
            powerButton.Click += (s, e) => GetRandomRole(RoleType.DPS);

            var condiButton = new StandardButton
            {
                Text = "Condi",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(0, 80)
            };
            condiButton.Click += (s, e) => GetRandomRole(RoleType.DPS);

            // Column 2
            var boonDpsButton = new StandardButton
            {
                Text = "BoonDPS",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(140, 0)
            };
            boonDpsButton.Click += (s, e) => GetRandomDPSWithBoon(true, false);

            var quicknessButton = new StandardButton
            {
                Text = "Quickness",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(140, 40)
            };
            quicknessButton.Click += (s, e) => GetRandomDPSWithBoon(true, false);

            var alacrityButton = new StandardButton
            {
                Text = "Alacrity",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(140, 80)
            };
            alacrityButton.Click += (s, e) => GetRandomDPSWithBoon(false, true);

            // Column 3
            var healerButton = new StandardButton
            {
                Text = "Healer",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(280, 0)
            };
            healerButton.Click += (s, e) => GetRandomRole(RoleType.Healer);

            var quickHealButton = new StandardButton
            {
                Text = "QuickHeal",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(280, 40)
            };
            quickHealButton.Click += (s, e) => GetRandomHealerWithBoon(true, false);

            var alacHealButton = new StandardButton
            {
                Text = "AlacHeal",
                Parent = gridPanel,
                Size = new Point(120, 32),
                Location = new Point(280, 80)
            };
            alacHealButton.Click += (s, e) => GetRandomHealerWithBoon(false, true);

            // Create role display panel
            _roleDisplayPanel = new Panel
            {
                Parent = mainContainer,
                Size = new Point(_mainWindow.ContentRegion.Width - 20, 150),
                BackgroundColor = new Color(0, 0, 0, 100),
                Visible = false
            };

            // Create icons panel
            var iconsPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 0),
                Parent = _roleDisplayPanel,
                Location = new Point(10, 10),
                Size = new Point(80, 40)
            };

            _professionIcon = new Image
            {
                Parent = iconsPanel,
                Size = new Point(32, 32),
                Texture = AsyncTexture2D.FromAssetId(156678) // Default icon
            };

            _eliteSpecIcon = new Image
            {
                Parent = iconsPanel,
                Size = new Point(32, 32),
                Texture = AsyncTexture2D.FromAssetId(156678) // Default icon
            };

            _boonIcon = new Image
            {
                Parent = iconsPanel,
                Size = new Point(32, 32),
                Texture = AsyncTexture2D.FromAssetId(156678), // Default icon
                Visible = false
            };

            // Create role info panel
            var roleInfoPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                Parent = _roleDisplayPanel,
                Location = new Point(100, 10),
                Size = new Point(_roleDisplayPanel.Width - 110, _roleDisplayPanel.Height - 20)
            };

            _roleNameLabel = new Label
            {
                Parent = roleInfoPanel,
                Text = "No role selected",
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                AutoSizeHeight = true,
                AutoSizeWidth = true
            };

            _roleDescriptionLabel = new Label
            {
                Parent = roleInfoPanel,
                Text = "Click a button above to get a role suggestion",
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                AutoSizeHeight = true,
                AutoSizeWidth = true
            };

            _buildUrlButton = new StandardButton
            {
                Parent = roleInfoPanel,
                Text = "View Build",
                Size = new Point(100, 25),
                Visible = false
            };
            _buildUrlButton.Click += (s, e) => OpenBuildUrl();
        }

        private void DisplayRole(RoleSuggestion role)
        {
            try
            {
                // Get profession and elite spec icons from the config
                var profession = RoleConfig.GetProfessionByName(role.Profession);
                var eliteSpec = RoleConfig.GetEliteSpecByName(role.EliteSpec);

                // Update profession icon
                if (profession != null && !string.IsNullOrEmpty(profession.Icon) && int.TryParse(profession.Icon, out int professionIconId))
                {
                    _professionIcon.Texture = AsyncTexture2D.FromAssetId(professionIconId);    
                }

                // Update elite spec icon
                if (eliteSpec != null && !string.IsNullOrEmpty(eliteSpec.Icon) && int.TryParse(eliteSpec.Icon, out int eliteSpecIconId))
                {
                    _eliteSpecIcon.Texture = AsyncTexture2D.FromAssetId(eliteSpecIconId);
                }

                // Update boon icon based on boon provision
                _boonIcon.Visible = false;
                if (role.ProvidesQuickness && role.ProvidesAlacrity)
                {
                    // Show quickness icon for dual boon providers (or could show both)
                    _boonIcon.Texture = AsyncTexture2D.FromAssetId(1012835); // Quickness icon
                    _boonIcon.Visible = true;
                }
                else if (role.ProvidesQuickness)
                {
                    _boonIcon.Texture = AsyncTexture2D.FromAssetId(1012835); // Quickness icon
                    _boonIcon.Visible = true;
                }
                else if (role.ProvidesAlacrity)
                {
                    _boonIcon.Texture = AsyncTexture2D.FromAssetId(1938787); // Alacrity icon
                    _boonIcon.Visible = true;
                }

                // Update role information
                _roleNameLabel.Text = $"{role.Profession} - {role.EliteSpec} ({role.Role})";
                _roleDescriptionLabel.Text = role.Description;

                // Show build URL button if available
                _buildUrlButton.Visible = !string.IsNullOrEmpty(role.BuildUrl);
                _currentBuildUrl = role.BuildUrl ?? string.Empty; // Store the URL for the button click

                // Show the role display panel
                _roleDisplayPanel.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to display role");
            }
        }

        private void OpenBuildUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentBuildUrl))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _currentBuildUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to open build URL");
                ScreenNotification.ShowNotification("Failed to open build URL", ScreenNotification.NotificationType.Error);
            }
        }

        private async void GetRandomRole(RoleType roleType = RoleType.FullRandom)
        {
            try
            {
                // Check if user has unlocked elite specs
                var hasUnlockedSpecs = await Gw2Api.HasUnlockedEliteSpecsAsync();
                
                if (!hasUnlockedSpecs)
                {
                    ScreenNotification.ShowNotification("No unlocked elite specs found. Please unlock some elite specializations first.", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Get available roles based on unlocked specs
                var availableRoles = await Gw2Api.GetAvailableRolesAsync(roleType);
                
                if (!availableRoles.Any())
                {
                    ScreenNotification.ShowNotification($"No available roles for {roleType} with your unlocked elite specs", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Pick a random role from available options
                var random = new Random();
                var randomIndex = random.Next(availableRoles.Count);
                var suggestion = availableRoles[randomIndex];
                
                // Display the role in the UI
                DisplayRole(suggestion);
                
                ScreenNotification.ShowNotification($"Try: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})", ScreenNotification.NotificationType.Info);
                
                Logger.Info($"Selected role: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to get random role");
                ScreenNotification.ShowNotification("Failed to get role suggestion", ScreenNotification.NotificationType.Error);
            }
        }

        private async void GetRandomDPSWithBoon(bool providesQuickness, bool providesAlacrity)
        {
            try
            {
                // Check if user has unlocked elite specs
                var hasUnlockedSpecs = await Gw2Api.HasUnlockedEliteSpecsAsync();
                
                if (!hasUnlockedSpecs)
                {
                    ScreenNotification.ShowNotification("No unlocked elite specs found. Please unlock some elite specializations first.", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Get available DPS roles with specific boon requirements
                var availableRoles = RoleConfig.GetDPSWithBoon(providesQuickness, providesAlacrity);
                
                if (!availableRoles.Any())
                {
                    var boonText = providesQuickness && providesAlacrity ? "both boons" : 
                                  providesQuickness ? "quickness" : "alacrity";
                    ScreenNotification.ShowNotification($"No available DPS roles with {boonText} and your unlocked elite specs", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Pick a random role from available options
                var random = new Random();
                var randomIndex = random.Next(availableRoles.Count);
                var suggestion = availableRoles[randomIndex];
                
                // Display the role in the UI
                DisplayRole(suggestion);
                
                ScreenNotification.ShowNotification($"Try: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})", ScreenNotification.NotificationType.Info);
                
                Logger.Info($"Selected DPS role with boons: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to get random DPS role with boon");
                ScreenNotification.ShowNotification("Failed to get role suggestion", ScreenNotification.NotificationType.Error);
            }
        }

        private async void GetRandomHealerWithBoon(bool providesQuickness, bool providesAlacrity)
        {
            try
            {
                // Check if user has unlocked elite specs
                var hasUnlockedSpecs = await Gw2Api.HasUnlockedEliteSpecsAsync();
                
                if (!hasUnlockedSpecs)
                {
                    ScreenNotification.ShowNotification("No unlocked elite specs found. Please unlock some elite specializations first.", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Get available healer roles with specific boon requirements
                var availableRoles = RoleConfig.GetHealerWithBoon(providesQuickness, providesAlacrity);
                
                if (!availableRoles.Any())
                {
                    var boonText = providesQuickness && providesAlacrity ? "both boons" : 
                                  providesQuickness ? "quickness" : "alacrity";
                    ScreenNotification.ShowNotification($"No available healer roles with {boonText} and your unlocked elite specs", ScreenNotification.NotificationType.Warning);
                    return;
                }

                // Pick a random role from available options
                var random = new Random();
                var randomIndex = random.Next(availableRoles.Count);
                var suggestion = availableRoles[randomIndex];
                
                // Display the role in the UI
                DisplayRole(suggestion);
                
                ScreenNotification.ShowNotification($"Try: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})", ScreenNotification.NotificationType.Info);
                
                Logger.Info($"Selected healer role with boons: {suggestion.Profession} - {suggestion.EliteSpec} ({suggestion.Role})");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to get random healer role with boon");
                ScreenNotification.ShowNotification("Failed to get role suggestion", ScreenNotification.NotificationType.Error);
            }
        }

        internal static Soeed.WhatAmIPlaying.WhatAmIPlayingModule WhatAmIPlayingModuleInstance;
    }
}
