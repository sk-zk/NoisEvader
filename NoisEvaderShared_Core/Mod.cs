using NoisEvader.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public struct Mod
    {
        public static Mod None => new Mod() { GameSpeed = 1 };
        
        public ModFlags Flags { get; set; }
        public float GameSpeed { get; set; }
        public float? TickRate { get; set; }

        public float Multiplier => GameSpeed; // TODO
         
        /// <summary>
        /// Gets a shorthand string of the mod for scoreboards.
        /// </summary>
        public override string ToString()
        {
            const string seperator = "|";
            var modAbbrevs = new List<string>();

            // hax
            if (Flags.HasFlag(ModFlags.Auto))
                modAbbrevs.Add("AT");
            if (Flags.HasFlag(ModFlags.Showcase))
                modAbbrevs.Add("SC");
            if (Flags.HasFlag(ModFlags.Zen))
                modAbbrevs.Add("ZN");

            // difficulty
            if (GameSpeed != 1)
                modAbbrevs.Add($"{GameSpeed}x");

            if (Flags.HasFlag(ModFlags.Flashlight))
                modAbbrevs.Add("FL");

            // other
            if (Flags.HasFlag(ModFlags.Live))
                modAbbrevs.Add("LV");

            // technical
            if (TickRate != null)
                modAbbrevs.Add($"{TickRate.Value / GameSpeed}T");

            if (Flags.HasFlag(ModFlags.NaiveWarp))
                modAbbrevs.Add("NW");

            return string.Join(seperator, modAbbrevs);
        }

        public static void ApplyLevelSettings(ref Mod mod, LevelSettings settings)
        {
            if (settings.ThirtyTicks)
                mod.TickRate = 30 * mod.GameSpeed;
            else
                mod.TickRate = null;

            SetMod(ref mod, ModFlags.NaiveWarp, settings.NaiveWarp);
        }

        public static void SetMod(ref Mod mod, ModFlags flag, bool enabled)
        {
            if (enabled)
                mod.Flags |= flag;
            else
                mod.Flags &= ~flag;
        }
    }

    [Flags]
    public enum ModFlags
    {
        None = 0,
        Auto = 1,
        Showcase = 2,
        Zen = 4,
        Live = 8,
        Flashlight = 16,
        NaiveWarp = 32,
    }
}
