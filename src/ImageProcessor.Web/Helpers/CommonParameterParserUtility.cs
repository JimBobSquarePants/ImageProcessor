// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonParameterParserUtility.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Web.Extensions;

    /// <summary>
    /// Encapsulates methods to correctly parse querystring parameters.
    /// </summary>
    public static class CommonParameterParserUtility
    {
        /// <summary>
        /// The collection of known colors.
        /// </summary>
        private static readonly Dictionary<string, KnownColor> KnownColors = new Dictionary<string, KnownColor>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The regular expression to search strings for colors.
        /// </summary>
        private static readonly Regex ColorRegex = BuildColorRegex();

        /// <summary>
        /// The regular expression to search strings for angles.
        /// </summary>
        private static readonly Regex AngleRegex = new Regex("(^(rotate(bounded)?|angle)|[^.](&,)?rotate(bounded)?|angle)(=|-)[^&|,]+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// The regular expression to search strings for values between 1 and 100.
        /// </summary>
        private static readonly Regex In100RangeRegex = new Regex("(-?0*(?:100|[1-9][0-9]?))", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Returns the correct <see cref="T:System.Int32"/> containing the angle for the given string.
        /// </summary>
        /// <param name="input">
        /// The input string containing the value to parse.
        /// </param>
        /// <returns>
        /// The correct <see cref="T:System.Int32"/> containing the angle for the given string.
        /// </returns>
        public static float ParseAngle(string input)
        {
            if (AngleRegex.Match(input) is var match && match.Success)
            {
                // Split on angle
                var value = match.Value;
                value = match.Value.IndexOf("angle", StringComparison.OrdinalIgnoreCase) >= 0
                    ? value.Substring(value.IndexOf('-') + 1)
                    : match.Value.Split('=')[1];

                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var angle);

                return angle;
            }

            // No rotate - matches the RotateLayer default.
            return 0;
        }

        /// <summary>
        /// Returns the correct <see cref="T:System.Drawing.Color"/> for the given string.
        /// </summary>
        /// <param name="input">
        /// The input string containing the value to parse.
        /// </param>
        /// <returns>
        /// The correct <see cref="T:System.Drawing.Color"/>
        /// </returns>
        public static Color ParseColor(string input)
        {
            if (ColorRegex.Match(input) is var match && match.Success)
            {
                var value = match.Value;

                if (KnownColors.TryGetValue(value, out var knownColor))
                {
                    return Color.FromKnownColor(knownColor);
                }

                if (value.Contains(","))
                {
                    var split = value.ToPositiveIntegerArray();
                    var red = split[0].ToByte();
                    var green = split[1].ToByte();
                    var blue = split[2].ToByte();
                    var alpha = split[3].ToByte();

                    return Color.FromArgb(alpha, red, green, blue);
                }

                // Split on color-hex
                return ColorTranslator.FromHtml("#" + value);
            }

            return Color.Transparent;
        }

        /// <summary>
        /// Returns the correct <see cref="T:System.Int32"/> for the given string.
        /// </summary>
        /// <param name="input">
        /// The input string containing the value to parse.
        /// </param>
        /// <returns>
        /// The correct <see cref="T:System.Int32"/> between -100 and 100.
        /// </returns>
        public static int ParseIn100Range(string input)
        {
            var value = 0;
            foreach (Match match in In100RangeRegex.Matches(input))
            {
                value = int.Parse(match.Value, CultureInfo.InvariantCulture);
            }

            return value;
        }

        /// <summary>
        /// Builds a regular expression for the three main colour types.
        /// </summary>
        /// <returns>
        /// The <see cref="Regex"/> to match colors.
        /// </returns>
        private static Regex BuildColorRegex()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(@"(\d+,\d+,\d+,\d+|([0-9a-f]{3}){1,2}|(");

            var knownColors = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            for (var i = 0; i < knownColors.Length; i++)
            {
                var knownColor = knownColors[i];
                var name = knownColor.ToString();

                KnownColors.Add(name, knownColor);

                if (i > 0)
                {
                    stringBuilder.Append('|');
                }

                stringBuilder.Append(Regex.Escape(name));
            }

            stringBuilder.Append("))");

            return new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }
    }
}
