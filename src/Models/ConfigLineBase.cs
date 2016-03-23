using SharpDX;

namespace PoeHUD.Models
{
    public abstract class ConfigLineBase
    {
        public string Text { get; set; }
        public Color? Color { get; set; }

        public override bool Equals(object obj) => Text == ((ConfigLineBase)obj).Text;

        public override int GetHashCode() => Text.GetHashCode();
    }
}