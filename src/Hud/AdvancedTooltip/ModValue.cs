﻿using PoeHUD.Controllers;
using PoeHUD.Framework;
using PoeHUD.Poe.FilesInMemory;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoeHUD.Hud.AdvancedTooltip
{
    public class ModValue
    {
        private readonly int totalTiers = 1;

        public ModValue(ItemMod mod, FsController fs, int iLvl)
        {
            string name = mod.RawName;
            Record = fs.Mods.records[name];
            AffixType = Record.AffixType;
            AffixText = String.IsNullOrEmpty(Record.UserFriendlyName) ? Record.Key : Record.UserFriendlyName;
            IsCrafted = Record.Domain == 10;
            StatValue = new[] { mod.Value1, mod.Value2, mod.Value3, mod.Value4 };
            Tier = -1;

            int subOptimalTierDistance = 0;

            List<ModsDat.ModRecord> allTiers;
            if (fs.Mods.recordsByTier.TryGetValue(Tuple.Create(Record.Group, Record.AffixType), out allTiers))
            {
                IEnumerable<string> validTags = Record.Tags.Select(t => t.Key)
                    .Where((t, tIdx) => Record.TagChances
                    .Where((c, cIdx) => tIdx == cIdx && c > 0).Any());
                bool tierFound = false;
                totalTiers = 0;
                foreach (ModsDat.ModRecord record in allTiers.Where(record => record.Tags.Where((t, tIdx) => record.TagChances
                    .Where((c, cIdx) => tIdx == cIdx && c > 0 && (t.Key == "default" || validTags.Contains(t.Key)))
                    .Any()).Any()).Where(record => record.StatNames[0] == Record.StatNames[0] && record.StatNames[1] == Record.StatNames[1]
                                                   && record.StatNames[2] == Record.StatNames[2] && record.StatNames[3] == Record.StatNames[3]))
                {
                    totalTiers++;
                    if (record.Equals(Record))
                    {
                        Tier = totalTiers;
                        tierFound = true;
                    }
                    if (!tierFound && record.MinLevel <= iLvl)
                    {
                        subOptimalTierDistance++;
                    }
                }
            }

            double hue = totalTiers == 1 ? 180 : 120 - Math.Min(subOptimalTierDistance, 3) * 40;
            Color = ColorUtils.ColorFromHsv(hue, totalTiers == 1 ? 0 : 1, 1);
        }

        public ModsDat.ModType AffixType { get; }
        public bool IsCrafted { get; }
        public String AffixText { get; }
        public Color Color { get; }
        public ModsDat.ModRecord Record { get; }
        public int[] StatValue { get; }
        public int Tier { get; }

        public bool CouldHaveTiers() => totalTiers > 1;
    }
}