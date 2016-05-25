using ImageProcessorCore.Samplers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessorCore.Web.Middleware
{
    public class ImageProcessorMiddleware
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly RequestDelegate _next;

        public ImageProcessorMiddleware(RequestDelegate next, IHostingEnvironment hostingEnvironment)
        {
            _next = next;
            _hostingEnvironment = hostingEnvironment;
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

            var inputPath = _hostingEnvironment.ContentRootPath + "/wwwroot" + context.Request.Path;

            using (var inputStream = File.OpenRead(inputPath))
            using (var outputStream = new MemoryStream())
            using (var image = new Image(inputStream))
            {
                image.Resize(width, height)
                    .Save(outputStream);

                var bytes = outputStream.ToArray();
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
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