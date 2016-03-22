using PoeHUD.Hud.Settings;
using SharpDX;

namespace PoeHUD.Hud.Trackers
{
    public sealed class PoiTrackerSettings : SettingsBase
    {
        public PoiTrackerSettings()
        {
            Enable = true;
            Masters = true;
            Strongboxes = true;
            Chests = true;
            MastersIcon = new RangeNode<int>(8, 1, 16);
            StrongboxesIcon = new RangeNode<int>(12, 1, 24);
            ChestsIcon = new RangeNode<int>(3, 1, 6);
            Cadiro = true;
            CadiroIcon = new RangeNode<int>(12, 1, 24);
            PerandusChestIcon = new RangeNode<int>(12, 1, 24);
            PerandusChestColor = new ColorBGRA(192, 192, 192, 230);
            PerandusChest = true;
        }

        public ToggleNode Masters { get; set; }
        public ToggleNode Strongboxes { get; set; }
        public ToggleNode Chests { get; set; }
        public RangeNode<int> MastersIcon { get; set; }
        public RangeNode<int> StrongboxesIcon { get; set; }
        public RangeNode<int> ChestsIcon { get; set; }
        public ToggleNode Cadiro { get; set; }
        public RangeNode<int> CadiroIcon { get; set; }
        public RangeNode<int> PerandusChestIcon { get; set; }
        public ColorBGRA PerandusChestColor { get; set; }
        public ToggleNode PerandusChest { get; set; }
    }
}