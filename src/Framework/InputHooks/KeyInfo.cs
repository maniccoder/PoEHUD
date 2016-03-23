using System.Windows.Forms;

namespace PoeHUD.Framework.InputHooks
{
    public class KeyInfo
    {
        public KeyInfo(Keys keys, bool control, bool alt, bool shift)
        {
            Keys = keys;
            Control = control;
            Alt = alt;
            Shift = shift;
        }

        public Keys Keys { get; }
        public bool Control { get; }
        public bool Alt { get; }
        public bool Shift { get; }
        public bool Handled { get; set; }
    }
}