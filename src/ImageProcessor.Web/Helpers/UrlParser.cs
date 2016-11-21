namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Linq;
    using ImageProcessor.Web.Extensions;

    public class UrlParser
    {
        public static void ParseUrl(string url, string servicePrefix, out string requestPath, out string queryString)
        {
            // Remove any service identifier prefixes from the url.
            if (!string.IsNullOrWhiteSpace(servicePrefix))
            {
                url = url.Split(new[] { servicePrefix }, StringSplitOptions.None)[1].TrimStart("?");
            }

            //Workaround for handling entirely encoded path for https://github.com/JimBobSquarePants/ImageProcessor/issues/478
            //If url does not contain a query delimiter but does contain an encoded questionmark, 
            //treat the last encoded questionmark as the query delimiter
            if (url.IndexOf('?') == -1 && url.IndexOf("%3F") > 0)
            {
                int idx = url.LastIndexOf("%3F");
                url = url.Remove(idx, 3).Insert(idx, "?");
            }

            // Identify each part of the incoming request.
            int queryCount = url.Count(f => f == '?');
            bool hasParams = queryCount > 0;
            bool hasMultiParams = queryCount > 1;
            string[] splitPath = url.Split('?');

            // Ensure we include any relevent querystring parameters into our request path for third party requests.
            requestPath = hasMultiParams ? string.Join("?", splitPath.Take(splitPath.Length - 1)) : splitPath[0];
            queryString = hasParams ? splitPath[splitPath.Length - 1] : string.Empty;

            //Url decode passed request path #506
            //Use Uri.UnescapeDataString instead of HttpUtility.UrlDecode to maintain plus-characters (+)
            requestPath = Uri.UnescapeDataString(requestPath);
        }
    }
}
