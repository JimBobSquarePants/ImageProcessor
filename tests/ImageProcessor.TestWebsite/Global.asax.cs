using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ImageProcessor.Web.HttpModules;

namespace ImageProcessor.TestWebsite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Register the event handler here.
            //ImageProcessingModule.ValidatingRequest += (sender, args) =>
            //{
            //    if (!string.IsNullOrWhiteSpace(args.QueryString))
            //    {
            //        //  Only allowed known parameters
            //        NameValueCollection queryCollection = HttpUtility.ParseQueryString(args.QueryString);

            //        // Ignore all but allowed querystrings.
            //        string[] allowed = { "width", "height" };
            //        IEnumerable<string> match = queryCollection.AllKeys.Intersect(allowed, StringComparer.OrdinalIgnoreCase);
            //        if (!match.Any())
            //        {
            //            args.Cancel = true;
            //        }
            //    }
            //};

            // Test the post processing event.
            //ImageProcessingModule.OnPostProcessing += (sender, args) => Debug.WriteLine(args.ImageExtension);

            //ImageProcessingModule.OnProcessQuerystring += (sender, args) =>
            //{
            //    if (!args.RawUrl.Contains("penguins"))
            //    {
            //        return args.Querystring += "watermark=protected&color=fff&fontsize=36&fontopacity=70textshadow=true&fontfamily=arial";
            //    }

            //    return args.Querystring;
            //};
        }
    }
}
