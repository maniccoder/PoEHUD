using System.Windows.Forms;

using PointDX = SharpDX.Point;
using PointGdi = System.Drawing.Point;

namespace PoeHUD.Framework.InputHooks
{
    public sealed class MouseInfo
    {
        public MouseInfo(MouseButtons buttons, PointGdi position, int wheelDelta)
        {
            Buttons = buttons;
            Position = new PointDX(position.X, position.Y);
            WheelDelta = wheelDelta;
        }

        public MouseButtons Buttons { get; }
        public PointDX Position { get; }
        public int WheelDelta { get; }
        public bool Handled { get; set; }
    }
}