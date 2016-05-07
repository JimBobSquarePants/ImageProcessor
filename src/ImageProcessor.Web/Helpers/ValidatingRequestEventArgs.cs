using System.ComponentModel;
using System.Web;

namespace ImageProcessor.Web.Helpers
{
    /// <summary>
    /// The validating request event args
    /// </summary>
    /// <remarks>
    /// This can be used by event subscribers to cancel image processing based on the information contained in the 
    /// request, or can be used to directly manipulate the querystring parameter that will be used to process the image.
    /// </remarks>
    public class ValidatingRequestEventArgs : CancelEventArgs
    {
        public ValidatingRequestEventArgs(HttpContextBase http, string queryString)
        {
            Context = http;
            QueryString = queryString;
        }

        /// <summary>
        /// Gets the current http context.
        /// </summary>
        public HttpContextBase Context { get; private set; }

        /// <summary>
        /// Gets/sets the query string
        /// </summary>
        /// <remarks>
        /// Event subscribers can directly manipulate the querystring before it's used for image processing
        /// </remarks>
        public string QueryString { get; set; }
    }
}