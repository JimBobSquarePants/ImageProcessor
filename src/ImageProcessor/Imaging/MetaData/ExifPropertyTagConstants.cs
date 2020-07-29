// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExifPropertyTagConstants.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

namespace ImageProcessor.Imaging.MetaData
{
    /// <summary>
    /// Contains constants grouping together common property items.
    /// </summary>
    public static class ExifPropertyTagConstants
    {
        /// <summary>
        /// Gets all required property items. The Gif format specifically requires these properties.
        /// </summary>
        public static readonly ExifPropertyTag[] RequiredPropertyItems = {
            ExifPropertyTag.LoopCount, ExifPropertyTag.FrameDelay
        };

        /// <summary>
        /// Gets all required property items plus geolocation specific property items. 
        /// </summary>
        public static readonly ExifPropertyTag[] GeolocationPropertyItems = RequiredPropertyItems.Union(new[]{
            ExifPropertyTag.GpsAltitude,
            ExifPropertyTag.GpsAltitudeRef,
            ExifPropertyTag.GpsDestBear,
            ExifPropertyTag.GpsDestBearRef,
            ExifPropertyTag.GpsDestDist,
            ExifPropertyTag.GpsDestDistRef,
            ExifPropertyTag.GpsDestLat,
            ExifPropertyTag.GpsDestLatRef,
            ExifPropertyTag.GpsDestLong,
            ExifPropertyTag.GpsDestLongRef,
            ExifPropertyTag.GpsGpsDop,
            ExifPropertyTag.GpsGpsMeasureMode,
            ExifPropertyTag.GpsGpsSatellites,
            ExifPropertyTag.GpsGpsStatus,
            ExifPropertyTag.GpsGpsTime,
            ExifPropertyTag.GpsIFD,
            ExifPropertyTag.GpsImgDir,
            ExifPropertyTag.GpsImgDirRef,
            ExifPropertyTag.GpsLatitude,
            ExifPropertyTag.GpsLatitudeRef,
            ExifPropertyTag.GpsLongitude,
            ExifPropertyTag.GpsLongitudeRef,
            ExifPropertyTag.GpsMapDatum,
            ExifPropertyTag.GpsSpeed,
            ExifPropertyTag.GpsSpeedRef,
            ExifPropertyTag.GpsTrack,
            ExifPropertyTag.GpsTrackRef,
            ExifPropertyTag.GpsVer
        }).ToArray();

        /// <summary>
        /// Gets all required property items plus copyright specific property items. 
        /// </summary>
        public static readonly ExifPropertyTag[] CopyrightPropertyItems = RequiredPropertyItems.Union(new[]{
            ExifPropertyTag.Copyright,
            ExifPropertyTag.Artist,
            ExifPropertyTag.ImageTitle,
            ExifPropertyTag.ImageDescription,
            ExifPropertyTag.ExifUserComment,
            ExifPropertyTag.EquipMake,
            ExifPropertyTag.EquipModel,
            ExifPropertyTag.ThumbnailArtist,
            ExifPropertyTag.ThumbnailCopyRight,
            ExifPropertyTag.ThumbnailImageDescription,
            ExifPropertyTag.ThumbnailEquipMake,
            ExifPropertyTag.ThumbnailEquipModel,
        }).ToArray();

        /// <summary>
        /// Gets all required property items plus copyright and geolocation specific property items. 
        /// </summary>
        public static readonly ExifPropertyTag[] CopyrightAndGeolocationPropertyItems = GeolocationPropertyItems.Union(CopyrightPropertyItems).ToArray();

        /// <summary>
        /// Gets all known property items
        /// </summary>
        public static readonly ExifPropertyTag[] All = (ExifPropertyTag[])Enum.GetValues(typeof(ExifPropertyTag));

        /// <summary>
        /// Gets the ids of all valid EXIF property items.
        /// </summary>
        public static readonly int[] Ids = Enum.GetValues(typeof(ExifPropertyTag)).Cast<int>().ToArray();
    }
}
