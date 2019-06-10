using libtoxtmi.Utility;
using System;

namespace libtoxtmi.Models
{
    /// <summary>
    /// WIP Utilities for wrangling bits/cheering.
    /// </summary>
    public static class TmiBitsCheer
    {
        /// <summary>
        /// Bit cheer color level.
        /// </summary>
        public enum ColorLevel
        {
            Red = 10000,    // 10000+
            Blue = 5000,    // 5000 - 9999
            Green = 1000,   // 1000 - 4999
            Purple = 100,   // 100 - 999
            Gray = 1,       // 1 - 99 
            None = 0        // Invalid / no bits
        }

        /// <summary>
        /// Bit cheer theme variant.
        /// </summary>
        public enum Theme
        {
            Light = 0,
            Dark = 1
        }

        /// <summary>
        /// Bit cheer image display type / animation state.
        /// </summary>
        public enum Type
        {
            Animated = 0,
            Static = 1
        }

        /// <summary>
        /// Bit cheer emote sizes.
        /// </summary>
        public enum Size
        {
            SizeOne = 100,
            SizeOnePointFive = 150,
            SizeTwo = 200,
            SizeThree = 300,
            SizeFour = 400
        }

        /// <summary>
        /// Determines the appropriate cheer/emote color, based on the amount of bits cheered.
        /// </summary>
        public static ColorLevel GetColorLevelForBitsAmount(int bitsAmount)
        {
            if (bitsAmount >= (int)ColorLevel.Red)
                return ColorLevel.Red;
            if (bitsAmount >= (int)ColorLevel.Blue)
                return ColorLevel.Blue;
            if (bitsAmount >= (int)ColorLevel.Green)
                return ColorLevel.Green;
            if (bitsAmount >= (int)ColorLevel.Purple)
                return ColorLevel.Purple;
            if (bitsAmount >= (int)ColorLevel.Gray)
                return ColorLevel.Gray;

            return ColorLevel.None;
        }

        /// <summary>
        /// Generates a bit cheer emote URL.
        /// </summary>
        public static string GetEmoteUrl(Theme theme, Type type, ColorLevel color, Size size)
        {
            var themeVal = Enum.GetName(typeof(Theme), theme).ToLower();
            var typeVal = Enum.GetName(typeof(Type), type).ToLower();
            var colorVal = Enum.GetName(typeof(ColorLevel), color).ToLower();
            var sizeVal = TmiNumberFormatter.FormatEmoteSize(size);

            return $"https://static-cdn.jtvnw.net/bits/{themeVal}/{typeVal}/{colorVal}/{sizeVal}";
        }
    }
}