using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Http;
using System.IO;
using System.Collections.Generic;
using ImageProcessorCore.Samplers;
using ImageProcessorCore.Filters;
using System;
using System.Globalization;

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
                // move to the next request here?
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

                // write directly to the body output stream
                using (var outputStream = new MemoryStream())
                {
                    //image.Resize(width, height)
                    //    .Save(outputStream);

                    var processors = GetImageProcessorFilters(context.Request.Query);
                    if (width > 0 || height > 0)
                    {
                        image.Process(width, height, processors.ToArray())
                            .Save(outputStream);
                    }
                    else
                    {
                        image.Process(processors.ToArray())
                            .Save(outputStream);
                    }

                    var bytes = outputStream.ToArray();
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

            //await _next.Invoke(context);
        }

        private List<IImageProcessorCore> GetImageProcessorFilters(IReadableStringCollection queryString)
        {
            var processors = new List<IImageProcessorCore>();

            foreach (var qs in queryString)
            {
                switch (qs.Key)
                {
                    case "alpha":
                        if (IsValidAlphaValue(qs.Value))
                        {
                            processors.Add(new Alpha(Convert.ToInt32(qs.Value)));
                        }
                        break;
                    case "brightness":
                        if (IsValidBrightnessValue(qs.Value))
                        {
                            processors.Add(new Brightness(Convert.ToInt32(qs.Value)));
                        }
                        break;
                    case "hue":
                        if (IsValidHueValue(qs.Value))
                        {
                            processors.Add(new Hue(Convert.ToInt32(qs.Value)));
                        }
                        break;
                }
            }

            return processors;
        }

        public bool IsImageProcessorRequest(HttpContext context)
        {
            if (!IsImageExtension(Path.GetExtension(context.Request.Path.Value).ToLower()))
                return false;

            var isImageProcessorRequest = false;
            foreach (var qs in context.Request.Query)
            {
                switch (qs.Key.ToLower())
                {
                    case "width":
                    case "height":
                    case "alpha":
                    case "brightness":
                    case "hue":
                        isImageProcessorRequest = true;
                        break;
                }
            }

            return isImageProcessorRequest;
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
                    isImage = true;
                    break;
                default:
                    isImage = false;
                    break;
            }
            return isImage;
        }

        #region QueryString value validation

        private bool IsValidAlphaValue(object value)
        {
            if (value == null)
                return false;

            int number;
            var isInt = int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
            if (!isInt)
                return false;

            if (number > 100 || number < 0)
                return false;

            return true;
        }

        private bool IsValidBrightnessValue(object value)
        {
            if (value == null)
                return false;

            int number;
            var isInt = int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
            if (!isInt)
                return false;

            if (number > 100 || number < 0)
                return false;

            return true;
        }

        private bool IsValidHueValue(object value)
        {
            if (value == null)
                return false;

            int number;
            var isInt = int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
            if (!isInt)
                return false;

            if (number > 360 || number < 0)
                return false;

            return true;
        }

        #endregion
    }
}