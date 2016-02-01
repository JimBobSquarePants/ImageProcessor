using System;
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
            if (!IsImageRequest(context))
            {
                // move to the next request here?
                await _next.Invoke(context);
                return;
            }

            try
            {
                //var sb = new StringBuilder();
                //sb.AppendLine("=====================================================================================================");
                //sb.AppendLine("Date/Time: " + DateTime.UtcNow.ToString("M/d/yyyy hh:mm tt"));

                int width = 0;
                int.TryParse(context.Request.Query["width"], out width);
                int height = 0;
                int.TryParse(context.Request.Query["height"], out height);

                var inputPath = _appEnvironment.ApplicationBasePath + "/wwwroot" + context.Request.Path;
                using (var inputStream = File.OpenRead(inputPath))
                {
                    //sb.AppendLine("Input Path: " + inputPath);
                    //sb.AppendLine("Querystring Dimensions: " + width.ToString() + "x" + height.ToString());

                    //var sw = new Stopwatch();
                    //sw.Start();
                    var image = new Image(inputStream);
                    if (image.Width > image.Height && width != 0)
                        height = (width * image.Height) / image.Width;
                    if (image.Height > image.Width && height != 0)
                        width = (height * image.Width) / image.Height;
                    //sw.Stop();

                    //sb.AppendLine("Original Dimensions: " + image.Width.ToString() + "x" + image.Height.ToString());
                    //sb.AppendLine("Output Dimensions: " + width.ToString() + "x" + height.ToString());
                    //sb.AppendLine("Image Read Time in Seconds: " + sw.Elapsed.TotalSeconds.ToString());

                    // write directly to the body output stream
                    using (var outputStream = new MemoryStream())
                    {
                        //sw.Restart();
                        image.Resize(width, height)
                            .Save(outputStream);
                        var bytes = outputStream.ToArray();
                        //sw.Stop();
                        //sb.AppendLine("Image Processing Time in Seconds: " + sw.Elapsed.TotalSeconds.ToString());

                        //context.Response.Body.Write(bytes, 0, bytes.Length);
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    }

                    //// the following approach will write to disk then serve the image
                    ////var inputPath = _appEnvironment.ApplicationBasePath + "/wwwroot" + context.Request.Path;
                    //var outputPath = _appEnvironment.ApplicationBasePath + "/Uploads/" + Path.GetFileName(context.Request.Path);
                    //using (var outputStream = File.OpenWrite(outputPath))
                    //{
                    //    image.Resize(width, height)
                    //        .Save(outputStream);
                    //}
                    //var bytes = File.ReadAllBytes(outputPath);
                    //await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }

                //var logFilePath = _appEnvironment.ApplicationBasePath + "/Logs/ThumbnailLog-" + DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss") + ".txt";
                //var logFilePath = _appEnvironment.ApplicationBasePath + "/Logs/ThumbnailLog.txt";
                //var logWriter = new LogWriter(logFilePath);
                //logWriter.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                //var logFilePath = _appEnvironment.ApplicationBasePath + "/Logs/Exceptions.txt";
                //var logWriter = new LogWriter(logFilePath);
                //logWriter.WriteLine("=====================================================================================================");
                //logWriter.WriteLine(ex.ToString());
                throw new Exception(ex.ToString());
            }

            //await _next.Invoke(context);
        }

        public bool IsImageRequest(HttpContext context)
        {
            if (!IsImageExtension(Path.GetExtension(context.Request.Path.Value).ToLower()))
                return false;

            if (string.IsNullOrWhiteSpace(context.Request.Query["width"]) && string.IsNullOrWhiteSpace(context.Request.Query["height"]))
                return false;

            return true;
        }

        private bool IsImageExtension(string extension)
        {
            var isImage = false;
            switch (extension)
            {
                case ".bmp":
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".tif":
                    isImage = true;
                    break;
                default:
                    isImage = false;
                    break;
            }
            return isImage;
        }
    }
}