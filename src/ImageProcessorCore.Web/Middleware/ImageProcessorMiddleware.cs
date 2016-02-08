using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Http;
using System.IO;
using ImageProcessorCore.Samplers;

namespace ImageProcessorCore.Web.Middleware
{
    public class ImageProcessorMiddleware
    {
        private readonly IApplicationEnvironment _appEnvironment;

        private readonly RequestDelegate _next;

        public ImageProcessorMiddleware(RequestDelegate next, IApplicationEnvironment appEnvironment)
        {
            _next = next;
            _appEnvironment = appEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsImageProcessorRequest(context))
            {
                await _next.Invoke(context);
                return;
            }

            int width = 0;
            int.TryParse(context.Request.Query["width"], out width);
            int height = 0;
            int.TryParse(context.Request.Query["height"], out height);

            var inputPath = _appEnvironment.ApplicationBasePath + "/wwwroot" + context.Request.Path;
            using (var inputStream = File.OpenRead(inputPath))
            {
                var image = new Image(inputStream);

                using (var outputStream = new MemoryStream())
                {
                    image.Resize(width, height)
                        .Save(outputStream);

                    var bytes = outputStream.ToArray();
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
            }
        }

        public bool IsImageProcessorRequest(HttpContext context)
        {
            if (!IsImageExtension(Path.GetExtension(context.Request.Path.Value).ToLower()))
                return false;

            var isImageProcessorRequest = false;
            if (!string.IsNullOrWhiteSpace(context.Request.Query["width"]) || !string.IsNullOrWhiteSpace(context.Request.Query["height"]))
                isImageProcessorRequest = true;

            return isImageProcessorRequest;
        }

        private bool IsImageExtension(string extension)
        {
            var isImageExtension = false;
            switch (extension)
            {
                case ".bmp":
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".png":
                    isImageExtension = true;
                    break;
                default:
                    isImageExtension = false;
                    break;
            }
            return isImageExtension;
        }
    }
}