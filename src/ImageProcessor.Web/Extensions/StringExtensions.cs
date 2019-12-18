// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to the <see cref="T:System.String" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Extensions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="T:System.String"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// The regular expression to search strings for an array of positive floats.
        /// </summary>
        private static readonly Regex PositiveFloatArrayRegex = new Regex(@"[\d+\.]+(?=[,-])|[\d+\.]+(?![,-])", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>
        /// The regular expression to search strings for an array of positive integers.
        /// </summary>
        private static readonly Regex PositiveIntegerArrayRegex = new Regex(@"[\d+]+(?=[,-])|[\d+]+(?![,-])", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Creates an MD5 fingerprint of the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An MD5 fingerprint of the String.</returns>
        public static string ToMD5Fingerprint(this string expression)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(expression.ToCharArray());

            using (var md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash = md5.ComputeHash(bytes);

                // Concatenate the hash bytes into one long String.
                return hash.Aggregate(
                    new StringBuilder(32),
                    (sb, b) => sb.Append(b.ToString("x2", CultureInfo.InvariantCulture)))
                    .ToString();
            }
        }

        /// <summary>
        /// Creates an SHA1 fingerprint of the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An SHA1 fingerprint of the String.</returns>
        public static string ToSHA1Fingerprint(this string expression)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(expression.ToCharArray());

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] hash = sha1.ComputeHash(bytes);

                // Concatenate the hash bytes into one long String.
                return hash.Aggregate(
                    new StringBuilder(40),
                    (sb, b) => sb.Append(b.ToString("x2", CultureInfo.InvariantCulture)))
                    .ToString();
            }
        }

        /// <summary>
        /// Creates an array of integers scraped from the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An array of integers scraped from the String.</returns>
        public static int[] ToPositiveIntegerArray(this string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var matchCollection = PositiveIntegerArrayRegex.Matches(expression);

            // Get the collections.
            int count = matchCollection.Count;
            int[] matches = new int[count];

            // Loop and parse the int values.
            for (int i = 0; i < count; i++)
            {
                matches[i] = int.Parse(matchCollection[i].Value, CultureInfo.InvariantCulture);
            }

            return matches;
        }

        /// <summary>
        /// Creates an array of floats scraped from the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An array of floats scraped from the String.</returns>
        public static float[] ToPositiveFloatArray(this string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var matchCollection = PositiveFloatArrayRegex.Matches(expression);

            // Get the collections.
            int count = matchCollection.Count;
            float[] matches = new float[count];

            // Loop and parse the int values.
            for (int i = 0; i < count; i++)
            {
                matches[i] = float.Parse(matchCollection[i].Value, CultureInfo.InvariantCulture);
            }

            return matches;
        }

        /// <summary>
        /// Checks the string to see whether the value is a valid virtual path name.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>True if the given string is a valid virtual path name</returns>
        public static bool IsValidVirtualPathName(this string expression) => Uri.TryCreate(expression, UriKind.Relative, out Uri uri) && uri.IsWellFormedOriginalString();

        /// <summary>
        /// Trims a specified string from the start of another string.
        /// </summary>
        /// <param name="target">The target string</param>
        /// <param name="trimString">The string to trim from the start</param>
        /// <returns>Returns the trimmed string</returns>
        public static string TrimStart(this string target, string trimString)
        {
            string result = target;
            while (result.StartsWith(trimString, StringComparison.InvariantCultureIgnoreCase))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
    }
}
