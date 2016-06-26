using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor.Web.Helpers
{
    /// <summary>
    /// String Helper
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Trims a specified string from the start of another string.
        /// </summary>
        /// <param name="target">The target string</param>
        /// <param name="trimString">The string to trim from the start</param>
        /// <returns>Returns the trimmed string</returns>
        public static string TrimStart(this string target, string trimString)
        {
            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
    }
}
