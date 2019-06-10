using libtoxtmi.Models;
using System.Globalization;

namespace libtoxtmi.Utility
{
    /// <summary>
    /// Utility for dealing with number formatting.
    /// </summary>
    public static class TmiNumberFormatter
    {
        private static NumberFormatInfo SizeCodeNumberFormat
        {
            get
            {
                return new NumberFormatInfo()
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 1
                };
            }
        }

        /// <summary>
        /// Correctly formats an emote size, for use in Emote URLs (from double value).
        /// </summary>
        public static string FormatEmoteSize(double value)
        {
            return value.ToString(SizeCodeNumberFormat);
        }

        /// <summary>
        /// Correctly formats an emote size, for use in Emote URLs (from float value).
        /// </summary >
        public static string FormatEmoteSize(float value)
        {
            return value.ToString(SizeCodeNumberFormat);
        }

        /// <summary>
        /// Correctly formats an emote size, for use in Emote URLs (from Cheer Size value).
        /// </summary>
        public static string FormatEmoteSize(TmiBitsCheer.Size value)
        {
            // NB: The Size enum values are 100 for 1.0, 150 for 1.5, etc.
            return FormatEmoteSize((float)value / 100.0f);
        }
    }
}