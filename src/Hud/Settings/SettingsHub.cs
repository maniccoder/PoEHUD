using Newtonsoft.Json;
using PoeHUD.Hud.AdvancedTooltip;
using PoeHUD.Hud.Dps;
using PoeHUD.Hud.Health;
using PoeHUD.Hud.Icons;
using PoeHUD.Hud.InventoryPreview;
using PoeHUD.Hud.KillCounter;
using PoeHUD.Hud.Loot;
using PoeHUD.Hud.Menu;
using PoeHUD.Hud.Preload;
using PoeHUD.Hud.Settings.Converters;
using PoeHUD.Hud.Trackers;
using PoeHUD.Hud.XpRate;
using System;
using System.IO;

namespace PoeHUD.Hud.Settings
{
    public sealed class SettingsHub
    {
        private const string SETTINGS_FILE_NAME = "config/settings.json";

        private static readonly JsonSerializerSettings jsonSettings;

        static SettingsHub()
        {
            jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new SortContractResolver(),
                Converters = new JsonConverter[]
                {
                    new ColorNodeConverter(),
                    new ToggleNodeConverter(),
                    new FileNodeConverter()
                }
            };
        }

        public SettingsHub()
        {
            MenuSettings = new MenuSettings();
            DpsMeterSettings = new DpsMeterSettings();
            MapIconsSettings = new MapIconsSettings();
            ItemAlertSettings = new ItemAlertSettings();
            AdvancedTooltipSettings = new AdvancedTooltipSettings();
            MonsterTrackerSettings = new MonsterTrackerSettings();
            PoiTrackerSettings = new PoiTrackerSettings();
            PreloadAlertSettings = new PreloadAlertSettings();
            XpRateSettings = new XpRateSettings();
            HealthBarSettings = new HealthBarSettings();
            InventoryPreviewSettings = new InventoryPreviewSettings();
            KillCounterSettings = new KillCounterSettings();
        }

        [JsonProperty("Menu")]
        public MenuSettings MenuSettings { get; }

        [JsonProperty("DPS meter")]
        public DpsMeterSettings DpsMeterSettings { get; }

        [JsonProperty("Map icons")]
        public MapIconsSettings MapIconsSettings { get; }

        [JsonProperty("Item alert")]
        public ItemAlertSettings ItemAlertSettings { get; }

        [JsonProperty("Advanced tooltip")]
        public AdvancedTooltipSettings AdvancedTooltipSettings { get; }

        [JsonProperty("Monster tracker")]
        public MonsterTrackerSettings MonsterTrackerSettings { get; }

        [JsonProperty("Poi tracker")]
        public PoiTrackerSettings PoiTrackerSettings { get; }

        [JsonProperty("Preload alert")]
        public PreloadAlertSettings PreloadAlertSettings { get; }

        [JsonProperty("XP per hour")]
        public XpRateSettings XpRateSettings { get; }

        [JsonProperty("Health bar")]
        public HealthBarSettings HealthBarSettings { get; }

        [JsonProperty("Inventory preview")]
        public InventoryPreviewSettings InventoryPreviewSettings { get; }

        [JsonProperty("Kills Counter")]
        public KillCounterSettings KillCounterSettings { get; }

        public static SettingsHub Load()
        {
            try
            {
                string json = File.ReadAllText(SETTINGS_FILE_NAME);
                return JsonConvert.DeserializeObject<SettingsHub>(json, jsonSettings);
            }
            catch
            {
                if (File.Exists(SETTINGS_FILE_NAME))
                {
                    string backupFileName = SETTINGS_FILE_NAME + DateTime.Now.Ticks;
                    File.Move(SETTINGS_FILE_NAME, backupFileName);
                }

                var settings = new SettingsHub();
                Save(settings);
                return settings;
            }
        }

        public static void Save(SettingsHub settings)
        {
            using (var stream = new StreamWriter(File.Create(SETTINGS_FILE_NAME)))
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented, jsonSettings);
                stream.Write(json);
            }
        }
    }
}