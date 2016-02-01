using Microsoft.AspNet.Builder;

namespace ImageProcessorCore.Web.Middleware
{
    public static class ImageProcessorExtensions
    {
        public static IApplicationBuilder UseImageProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageProcessorMiddleware>();
        }
    }
}