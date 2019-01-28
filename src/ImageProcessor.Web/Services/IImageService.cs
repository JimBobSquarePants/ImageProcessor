// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImageService.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines properties and methods for allowing retrieval of images from different sources.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    ///  Defines properties and methods for allowing retrieval of images from different sources.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        string Prefix { get; set; }

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        bool IsFileLocalService { get; }

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        Dictionary<string, string> Settings { get; set; }

        /// <summary>
        /// Gets or sets the white list of <see cref="System.Uri"/>. 
        /// </summary>
        Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        bool IsValidRequest(string path);

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">
        /// The value identifying the image to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="System.Byte"/> array containing the image data.
        /// </returns>
        Task<byte[]> GetImage(object id);
    }

    /// <summary>
    /// An extension to IImageService that allows the passing of the current request context.
    /// </summary>
    public interface IImageService2 : IImageService
    {
        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">
        /// The value identifying the image to fetch.
        /// </param>
        /// <param name="context">The request context.</param>
        /// <returns>
        /// The <see cref="byte"/> array containing the image data.
        /// </returns>
        Task<byte[]> GetImage(object id, HttpContext context);
    }
}
