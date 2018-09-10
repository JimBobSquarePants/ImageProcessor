// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayColor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Adds a color overlay to the current image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	using ImageProcessor.Common.Exceptions;
	using ImageProcessor.Imaging.Helpers;

	/// <summary>
	/// Adds a color overlay to the current image.
	/// </summary>
	public class OverlayColor : IGraphicsProcessor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OverlayColor"/> class.
		/// </summary>
		public OverlayColor() => this.Settings = new Dictionary<string, string>();

		/// <summary>
		/// Gets or sets the DynamicParameter.
		/// </summary>
		public dynamic DynamicParameter { get; set; }

		/// <summary>
		/// Gets or sets any additional settings required by the processor.
		/// </summary>
		public Dictionary<string, string> Settings { get; set; }

		/// <summary>
		/// Processes the image.
		/// </summary>
		/// <param name="factory">The current instance of the 
		/// <see cref="T:ImageProcessor.ImageFactory" /> class containing
		/// the image to process.</param>
		/// <returns>
		/// The processed image from the current instance of the <see cref="T:ImageProcessor.ImageFactory" /> class.
		/// </returns>
		public Image ProcessImage(ImageFactory factory)
		{
			Image image = factory.Image;

			try
			{
				Color overlayColor = this.DynamicParameter;
				if (overlayColor.A > 0)
				{
					using (var graphics = Graphics.FromImage(image))
					{
						GraphicsHelper.SetGraphicsOptions(graphics, true);

						// Fill with overlay color
						using (SolidBrush brush = new SolidBrush(overlayColor))
						{
							graphics.FillRectangle(brush, 0, 0, image.Width, image.Height);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
			}

			return image;
		}
	}
}
