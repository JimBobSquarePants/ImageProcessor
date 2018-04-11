// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppDataImageService.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The image service demonstrating how to pull images from the app_data folder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.TestWebsite.ImageServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.Services;

    /// <summary>
    /// The image service demonstrating how to pull images from the app_data folder.
    /// </summary>
    public class AppDataImageService : IImageService
    {
        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = "appdata.axd";

        /// <summary>
        /// Gets or sets a value indicating whether Prefix should be evaulated as a regular expression.
        /// </summary>
        public bool IsRegex { get; set; }

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        public bool IsFileLocalService => true;

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the white list of <see cref="System.Uri"/>. 
        /// </summary>
        public Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public bool IsValidRequest(string path)
        {
            return ImageHelpers.IsValidImageExtension(path.Split('&', '?')[0]);
        }

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">
        /// The value identifying the image to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="System.Byte"/> array containing the image data.
        /// </returns>
        public async Task<byte[]> GetImage(object id)
        {
            const string AppData = "~/App_Data/images";
            string imageRoot = HostingEnvironment.MapPath(AppData);

            if (imageRoot == null)
            {
                throw new HttpException(404, "No root path found to serve " + id);
            }

            // In this instance we are just processing a set path. 
            // If you are using the querystring params as a means of identifying the correct image
            // then you can do something with it here.
            string path = Path.Combine(imageRoot, id.ToString().Split('&', '?')[0]);
            byte[] buffer;

            // Check to see if the file exists.
            // ReSharper disable once AssignNullToNotNullAttribute
            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                throw new HttpException(404, "Nothing found at " + id);
            }

            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                buffer = new byte[file.Length];
                await file.ReadAsync(buffer, 0, (int)file.Length);
            }

            return buffer;
        }
    }
}