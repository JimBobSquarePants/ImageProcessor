// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlParser.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   A helper class for decoding and parsing request urls.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Linq;
    using ImageProcessor.Web.Extensions;

    /// <summary>
    /// A helper class for decoding and parsing request URLs.
    /// </summary>
    public class UrlParser
    {
        /// <summary>
        /// Parses the given URL adjusting the request path to a value that can then be interpreted by an  image service.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="servicePrefix">The service prefix.</param>
        /// <param name="requestPath">The request path.</param>
        /// <param name="queryString">The query string.</param>
        public static void ParseUrl(string url, string servicePrefix, out string requestPath, out string queryString)
        {
            // Remove any service identifier prefixes from the url.
            if (!string.IsNullOrWhiteSpace(servicePrefix))
            {
                url = url.Split(new[] { servicePrefix }, StringSplitOptions.None)[1].TrimStart("?");
            }

            // Workaround for handling entirely encoded path for https://github.com/JimBobSquarePants/ImageProcessor/issues/478
            // If url does not contain a query delimiter but does contain an encoded questionmark,
            // treat the last encoded questionmark as the query delimiter
            if (url.IndexOf('?') == -1 && url.IndexOf("%3F", StringComparison.Ordinal) > 0)
            {
                int idx = url.LastIndexOf("%3F", StringComparison.Ordinal);
                url = url.Remove(idx, 3).Insert(idx, "?");
            }

            // Identify each part of the incoming request.
            int queryCount = url.Count(f => f == '?');
            bool hasParams = queryCount > 0;
            bool hasMultiParams = queryCount > 1;
            string[] splitPath = url.Split('?');

            // Ensure we include any relevent querystring parameters into our request path for third party requests.
            // Url decode passed request path #506
            // Use Uri.UnescapeDataString instead of HttpUtility.UrlDecode to maintain plus-characters (+)
            requestPath = Uri.UnescapeDataString(splitPath[0]);
            queryString = hasParams ? splitPath[splitPath.Length - 1] : string.Empty;

            // Certain Facebook requests require ony the first part to be decoded.
            if (hasMultiParams)
            {
                StringBuilder sb = new StringBuilder(requestPath);
                for (int i = 1; i < splitPath.Length - 1; i++)
                {
                    sb.AppendFormat("?{0}", splitPath[i]);
                }

                requestPath = sb.ToString();
            }
        }
    }
}