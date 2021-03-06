﻿using SharpDX;

namespace PoeHUD.Hud.Settings
{
    public sealed class ColorNode
    {
        public ColorNode()
        {
        }

        public ColorNode(uint color)
        {
            Value = Color.FromAbgr(color);
        }

        public ColorNode(Color color)
        {
            Value = color;
        }

        public Color Value { get; set; }

        public static implicit operator Color(ColorNode node) => node.Value;

        public static implicit operator ColorNode(uint value) => new ColorNode(value);

        public static implicit operator ColorNode(Color value) => new ColorNode(value);

        public static implicit operator ColorNode(ColorBGRA value) => new ColorNode(value);
    }
}