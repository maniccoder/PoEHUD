using PoeHUD.Controllers;
using PoeHUD.Framework;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.UI;
using PoeHUD.Models;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoeHUD.Hud.Settings;

namespace PoeHUD.Hud.Preload
{
    public class PreloadAlertPlugin : SizedPlugin<PreloadAlertSettings>
    {
        private readonly HashSet<PreloadConfigLine> alerts;
        private readonly Dictionary<string, PreloadConfigLine> alertStrings;
        private int lastCount = 0;
        private int lastAddress = 0;
        private bool foundSpecificPerandusChest = false;
        private bool holdKey = false;
        public static Color AreaNameColor = new Color();
        private readonly SettingsHub settingsHub;

        public PreloadAlertPlugin(GameController gameController, Graphics graphics, PreloadAlertSettings settings, SettingsHub settingsHub)
            : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
            alerts = new HashSet<PreloadConfigLine>();
            alertStrings = LoadConfig("config/preload_alerts.txt");
            GameController.Area.OnAreaChange += OnAreaChange;
            AreaNameColor = Settings.AreaTextColor;
        }

        public Dictionary<string, PreloadConfigLine> LoadConfig(string path)
        {
            return LoadConfigBase(path, 3).ToDictionary(line => line[0], line =>
            {
                var preloadAlerConfigLine = new PreloadConfigLine
                {
                    Text = line[1],
                    Color = line.ConfigColorValueExtractor(2)
                };
                return preloadAlerConfigLine;
            });
        }

        public override void Render()
        {
            if (!holdKey && WinApi.IsKeyDown(Keys.F10))
            {
                holdKey = true;
                Settings.Enable.Value = !Settings.Enable.Value;
                SettingsHub.Save(settingsHub);
            }
            else if (holdKey && !WinApi.IsKeyDown(Keys.F10))
            {
                holdKey = false;
            }
            if (!Settings.Enable) { return; }
            //Parse();

            if (alerts.Count <= 0) return;
            Vector2 startPosition = StartDrawPointFunc();
            Vector2 position = startPosition;
            int maxWidth = 0;
            foreach (Size2 size in alerts
                .Select(preloadConfigLine => Graphics
                    .DrawText(preloadConfigLine.Text, Settings.TextSize, position, preloadConfigLine.FastColor?
                        .Invoke() ?? preloadConfigLine.Color ?? Settings.DefaultTextColor, FontDrawFlags.Right)))
            {
                maxWidth = Math.Max(size.Width, maxWidth);
                position.Y += size.Height;
            }
            if (maxWidth <= 0) return;
            var bounds = new RectangleF(startPosition.X - maxWidth - 45, startPosition.Y - 5,
                maxWidth + 50, position.Y - startPosition.Y + 10);
            Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
            Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
            Size = bounds.Size;
            Margin = new Vector2(0, 5);
        }

        private void ResetArea()
        {
            alerts.Clear();
            AreaNameColor = Settings.AreaTextColor;
            lastCount = 0;
            lastAddress = 0;
            foundSpecificPerandusChest = false;
        }

        private void OnAreaChange(AreaController area)
        {
            ResetArea();
        }

        private void Parse()
        {
            if (WinApi.IsKeyDown(Keys.F5)) // do a full refresh if F5 is hit
            {
                ResetArea();
            }
            Memory memory = GameController.Memory;
            int pFileRoot = memory.ReadInt(memory.AddressOfProcess + memory.offsets.FileRoot);
            int count = memory.ReadInt(pFileRoot + 0xC); // check how many files are loaded
            if (count > lastCount) // if the file count has changed, check the newly loaded files
            {
                int listIterator = 0;
                int areaChangeCount = GameController.Game.AreaChangeCount;
                if (lastAddress == 0)
                {
                    // start at the beginning
                    listIterator = memory.ReadInt(pFileRoot + 0x10);
                }
                else
                {
                    // start at the last read file
                    listIterator = lastAddress;
                }
                for (int i = 0; i < count; i++)
                {
                    listIterator = memory.ReadInt(listIterator);
                    if (listIterator == 0)
                    {
                        // address is null, something has gone wrong, start over
                        ResetArea();
                        return;
                    }
                    lastAddress = listIterator;
                    if (memory.ReadInt(listIterator + 0x8) == 0 || memory.ReadInt(listIterator + 0xC, 0x24) != areaChangeCount) continue;
                    string text = memory.ReadStringU(memory.ReadInt(listIterator + 8));
                    if (text.Contains('@')) { text = text.Split('@')[0]; }
                    CheckForPreload(text);
                }
            }
            lastCount = count;
        }

        private void CheckForPreload(string text)
        {
            if (alertStrings.ContainsKey(text))
            {
                alerts.Add(alertStrings[text]);
                return;
            }
            if (text.Contains("Metadata/Terrain/Doodads/vaal_sidearea_effects/soulcoaster.ao"))
            {
                if (Settings.CorruptedTitle)
                {
                    // using corrupted titles so set the color here, XpRatePlugin will grab the color to use when drawing the title.
                    AreaNameColor = Settings.CorruptedAreaColor;
                }
                else
                {
                    // not using corrupted titles, so throw it in a preload alert
                    alerts.Add(new PreloadConfigLine { Text = "Corrupted Area", FastColor = () => Settings.CorruptedAreaColor });
                }
                return;
            }

            Dictionary<string, PreloadConfigLine> PerandusLeague = new Dictionary<string, PreloadConfigLine>
            {
                {"Metadata/NPC/League/Cadiro", new PreloadConfigLine { Text = "Cadiro Trader", FastColor = () => Settings.CadiroTrader }},
                {"Metadata/Chests/PerandusChests/PerandusChestStandard", new PreloadConfigLine { Text = "Perandus Chest", FastColor = () => Settings.PerandusChestStandard }},
                {"Metadata/Chests/PerandusChests/PerandusChestRarity", new PreloadConfigLine { Text = "Perandus Cache", FastColor = () => Settings.PerandusChestRarity }},
                {"Metadata/Chests/PerandusChests/PerandusChestQuantity", new PreloadConfigLine { Text = "Perandus Hoard", FastColor = () => Settings.PerandusChestQuantity }},
                {"Metadata/Chests/PerandusChests/PerandusChestCoins", new PreloadConfigLine { Text = "Perandus Coffer", FastColor = () => Settings.PerandusChestCoins }},
                {"Metadata/Chests/PerandusChests/PerandusChestJewellery", new PreloadConfigLine { Text = "Perandus Jewellery Box", FastColor = () => Settings.PerandusChestJewellery }},
                {"Metadata/Chests/PerandusChests/PerandusChestGems", new PreloadConfigLine { Text = "Perandus Safe", FastColor = () => Settings.PerandusChestGems }},
                {"Metadata/Chests/PerandusChests/PerandusChestCurrency", new PreloadConfigLine { Text = "Perandus Treasury", FastColor = () => Settings.PerandusChestCurrency }},
                {"Metadata/Chests/PerandusChests/PerandusChestInventory", new PreloadConfigLine { Text = "Perandus Wardrobe", FastColor = () => Settings.PerandusChestInventory }},
                {"Metadata/Chests/PerandusChests/PerandusChestDivinationCards", new PreloadConfigLine { Text = "Perandus Catalogue", FastColor = () => Settings.PerandusChestDivinationCards }},
                {"Metadata/Chests/PerandusChests/PerandusChestKeepersOfTheTrove", new PreloadConfigLine { Text = "Perandus Trove", FastColor = () => Settings.PerandusChestKeepersOfTheTrove }},
                {"Metadata/Chests/PerandusChests/PerandusChestUniqueItem", new PreloadConfigLine { Text = "Perandus Locker", FastColor = () => Settings.PerandusChestUniqueItem }},
                {"Metadata/Chests/PerandusChests/PerandusChestMaps", new PreloadConfigLine { Text = "Perandus Archive", FastColor = () => Settings.PerandusChestMaps }},
                {"Metadata/Chests/PerandusChests/PerandusChestFishing", new PreloadConfigLine { Text = "Perandus Tackle Box", FastColor = () => Settings.PerandusChestFishing }},
                {"Metadata/Chests/PerandusChests/PerandusManorUniqueChest", new PreloadConfigLine { Text = "Cadiro's Locker", FastColor = () => Settings.PerandusManorUniqueChest }},
                {"Metadata/Chests/PerandusChests/PerandusManorCurrencyChest", new PreloadConfigLine { Text = "Cadiro's Treasury", FastColor = () => Settings.PerandusManorCurrencyChest }},
                {"Metadata/Chests/PerandusChests/PerandusManorMapsChest", new PreloadConfigLine { Text = "Cadiro's Archive", FastColor = () => Settings.PerandusManorMapsChest }},
                {"Metadata/Chests/PerandusChests/PerandusManorJewelryChest", new PreloadConfigLine { Text = "Cadiro's Jewellery Box", FastColor = () => Settings.PerandusManorJewelryChest }},
                {"Metadata/Chests/PerandusChests/PerandusManorDivinationCardsChest", new PreloadConfigLine { Text = "Cadiro's Catalogue", FastColor = () => Settings.PerandusManorDivinationCardsChest }},
                {"Metadata/Chests/PerandusChests/PerandusManorLostTreasureChest", new PreloadConfigLine { Text = "Grand Perandus Vault", FastColor = () => Settings.PerandusManorLostTreasureChest }}
            };
            PreloadConfigLine perandus_alert = PerandusLeague.Where(kv => text
                .StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (perandus_alert != null && Settings.PerandusBoxes)
            {
                foundSpecificPerandusChest = true;
                if (alerts.Contains(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard }))
                {
                    alerts.Remove(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard });
                }
                alerts.Add(perandus_alert);
                return;
            }
            if (Settings.PerandusBoxes && !foundSpecificPerandusChest && text.StartsWith("Metadata/Chests/PerandusChests"))
            {
                alerts.Add(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard });
            }

            Dictionary<string, PreloadConfigLine> Strongboxes = new Dictionary<string, PreloadConfigLine>
            {
                {"Metadata/Chests/StrongBoxes/Arcanist", new PreloadConfigLine { Text = "Arcanist's Strongbox", FastColor = () => Settings.ArcanistStrongbox }},
                {"Metadata/Chests/StrongBoxes/Artisan", new PreloadConfigLine { Text = "Artisan's Strongbox", FastColor = () => Settings.ArtisanStrongbox }},
                {"Metadata/Chests/StrongBoxes/Cartographer", new PreloadConfigLine { Text = "Cartographer's Strongbox", FastColor = () => Settings.CartographerStrongbox }},
                {"Metadata/Chests/StrongBoxes/Gemcutter", new PreloadConfigLine { Text = "Gemcutter's Strongbox", FastColor = () => Settings.GemcutterStrongbox }},
                {"Metadata/Chests/StrongBoxes/Jeweller", new PreloadConfigLine { Text = "Jeweller's Strongbox", FastColor = () => Settings.JewellerStrongbox }},
                {"Metadata/Chests/StrongBoxes/Arsenal", new PreloadConfigLine { Text = "Blacksmith's Strongbox", FastColor = () => Settings.BlacksmithStrongbox }},
                {"Metadata/Chests/StrongBoxes/Armory", new PreloadConfigLine { Text = "Armourer's Strongbox", FastColor = () => Settings.ArmourerStrongbox }},
                {"Metadata/Chests/StrongBoxes/Ornate", new PreloadConfigLine { Text = "Ornate Strongbox", FastColor = () => Settings.OrnateStrongbox }},
                {"Metadata/Chests/StrongBoxes/Large", new PreloadConfigLine { Text = "Large Strongbox", FastColor = () => Settings.LargeStrongbox }},
                {"Metadata/Chests/StrongBoxes/Strongbox", new PreloadConfigLine { Text = "Simple Strongbox", FastColor = () => Settings.SimpleStrongbox }},
                {"Metadata/Chests/CopperChests/CopperChestEpic3", new PreloadConfigLine { Text = "Epic Chest", FastColor = () => Settings.EpicStrongbox }},
                {"Metadata/Chests/StrongBoxes/PerandusBox", new PreloadConfigLine { Text = "Perandus Strongbox", FastColor = () => Settings.PerandusStrongbox }},
                {"Metadata/Chests/StrongBoxes/KaomBox", new PreloadConfigLine { Text = "Kaom Strongbox", FastColor = () => Settings.KaomStrongbox }},
                {"Metadata/Chests/StrongBoxes/MalachaisBox", new PreloadConfigLine { Text = "Malachai Strongbox", FastColor = () => Settings.MalachaiStrongbox }}
            };
            PreloadConfigLine _alert = Strongboxes.Where(kv => text
                .StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (_alert != null && Settings.Strongboxes)
            {
                alerts.Add(_alert);
                return;
            }

            Dictionary<string, PreloadConfigLine> Preload = new Dictionary<string, PreloadConfigLine>
            {
                {"Wild/StrDexInt", new PreloadConfigLine { Text = "Zana, Master Cartographer", FastColor = () => Settings.MasterZana }},
                {"Wild/Int", new PreloadConfigLine { Text = "Catarina, Master of the Dead", FastColor = () => Settings.MasterCatarina }},
                {"Wild/Dex", new PreloadConfigLine { Text = "Tora, Master of the Hunt", FastColor = () => Settings.MasterTora }},
                {"Wild/DexInt", new PreloadConfigLine { Text = "Vorici, Master Assassin", FastColor = () => Settings.MasterVorici }},
                {"Wild/Str", new PreloadConfigLine { Text = "Haku, Armourmaster", FastColor = () => Settings.MasterHaku }},
                {"Wild/StrInt", new PreloadConfigLine { Text = "Elreon, Loremaster", FastColor = () => Settings.MasterElreon }},
                {"Wild/Fish", new PreloadConfigLine { Text = "Krillson, Master Fisherman", FastColor = () => Settings.MasterKrillson }},
                {"MasterStrDex1", new PreloadConfigLine { Text = "Vagan, Weaponmaster (2HSword)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex2", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Staff)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex3", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Bow)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex4", new PreloadConfigLine { Text = "Vagan, Weaponmaster (DaggerRapier)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex5", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Blunt)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex6", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Blades)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex7", new PreloadConfigLine { Text = "Vagan, Weaponmaster (SwordAxe)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex8", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Punching)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex9", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Flickerstrike)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex10", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Elementalist)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex11", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Cyclone)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex12", new PreloadConfigLine { Text = "Vagan, Weaponmaster (PhysSpells)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex13", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Traps)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex14", new PreloadConfigLine { Text = "Vagan, Weaponmaster (RighteousFire)", FastColor = () => Settings.MasterVagan }},
                {"MasterStrDex15", new PreloadConfigLine { Text = "Vagan, Weaponmaster (CastOnHit)", FastColor = () => Settings.MasterVagan }},
                {"ExileRanger1", new PreloadConfigLine { Text = "Exile Orra Greengate", FastColor = () => Settings.OrraGreengate }},
                {"ExileRanger2", new PreloadConfigLine { Text = "Exile Thena Moga", FastColor = () => Settings.ThenaMoga }},
                {"ExileRanger3", new PreloadConfigLine { Text = "Exile Antalie Napora", FastColor = () => Settings.AntalieNapora }},
                {"ExileDuelist1", new PreloadConfigLine { Text = "Exile Torr Olgosso", FastColor = () => Settings.TorrOlgosso }},
                {"ExileDuelist2", new PreloadConfigLine { Text = "Exile Armios Bell", FastColor = () => Settings.ArmiosBell }},
                {"ExileDuelist4", new PreloadConfigLine { Text = "Exile Zacharie Desmarais", FastColor = () => Settings.ZacharieDesmarais }},
                {"ExileWitch1", new PreloadConfigLine { Text = "Exile Minara Anenima", FastColor = () => Settings.MinaraAnenima }},
                {"ExileWitch2", new PreloadConfigLine { Text = "Exile Igna Phoenix", FastColor = () => Settings.IgnaPhoenix }},
                {"ExileMarauder1", new PreloadConfigLine { Text = "Exile Jonah Unchained", FastColor = () => Settings.JonahUnchained }},
                {"ExileMarauder2", new PreloadConfigLine { Text = "Exile Damoi Tui", FastColor = () => Settings.DamoiTui }},
                {"ExileMarauder3", new PreloadConfigLine { Text = "Exile Xandro Blooddrinker", FastColor = () => Settings.XandroBlooddrinker }},
                {"ExileMarauder5", new PreloadConfigLine { Text = "Exile Vickas Giantbone", FastColor = () => Settings.VickasGiantbone }},
                {"ExileTemplar1", new PreloadConfigLine { Text = "Exile Eoin Greyfur", FastColor = () => Settings.EoinGreyfur }},
                {"ExileTemplar2", new PreloadConfigLine { Text = "Exile Tinevin Highdove", FastColor = () => Settings.TinevinHighdove }},
                {"ExileTemplar4", new PreloadConfigLine { Text = "Exile Magnus Stonethorn", FastColor = () => Settings.MagnusStonethorn}},
                {"ExileShadow1_", new PreloadConfigLine { Text = "Exile Ion Darkshroud", FastColor = () => Settings.IonDarkshroud}},
                {"ExileShadow2", new PreloadConfigLine { Text = "Exile Ash Lessard", FastColor = () => Settings.AshLessard}},
                {"ExileShadow4", new PreloadConfigLine { Text = "Exile Wilorin Demontamer", FastColor = () => Settings.WilorinDemontamer}},
                {"ExileScion2", new PreloadConfigLine { Text = "Exile Augustina Solaria", FastColor = () => Settings.AugustinaSolaria}},
                {"ExileWitch4", new PreloadConfigLine { Text = "Exile Dena Lorenni", FastColor = () => Settings.DenaLorenni }},
                {"ExileScion4", new PreloadConfigLine { Text = "Exile Vanth Agiel", FastColor = () => Settings.VanthAgiel }},
                {"ExileScion3", new PreloadConfigLine { Text = "Exile Lael Furia", FastColor = () => Settings.LaelFuria }}
            };
            PreloadConfigLine alert = Preload.Where(kv => text
                .EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (alert != null && Settings.Exiles)
            {
                alerts.Add(alert);
                return;
            }
        }
    }
}