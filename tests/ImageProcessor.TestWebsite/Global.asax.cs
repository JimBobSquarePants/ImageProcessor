using System.Diagnostics;
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
