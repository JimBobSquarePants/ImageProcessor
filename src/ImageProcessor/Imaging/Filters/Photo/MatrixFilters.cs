// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixFilters.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The filters available to the Filter <see cref="IGraphicsProcessor" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.Photo
{
    using ImageProcessor.Processors;

    /// <summary>
    /// The filters available to the Filter <see cref="IGraphicsProcessor"/>.
    /// </summary>
    public static class MatrixFilters
    {
        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the black and white filter.
        /// </summary>
        public static IMatrixFilter BlackWhite => new BlackWhiteMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the comic filter.
        /// </summary>
        public static IMatrixFilter Comic => new ComicMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the gotham filter.
        /// </summary>
        public static IMatrixFilter Gotham => new GothamMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the greyscale filter.
        /// </summary>
        public static IMatrixFilter GreyScale => new GreyScaleMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the high saturation filter.
        /// </summary>
        public static IMatrixFilter HiSatch => new HiSatchMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the invert filter.
        /// </summary>
        public static IMatrixFilter Invert => new InvertMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the lomograph filter.
        /// </summary>
        public static IMatrixFilter Lomograph => new LomographMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the low saturation filter.
        /// </summary>
        public static IMatrixFilter LoSatch => new LoSatchMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the polaroid filter.
        /// </summary>
        public static IMatrixFilter Polaroid => new PolaroidMatrixFilter();

        /// <summary>
        /// Gets the <see cref="IMatrixFilter"/> for generating the sepia filter.
        /// </summary>
        public static IMatrixFilter Sepia => new SepiaMatrixFilter();
    }
}