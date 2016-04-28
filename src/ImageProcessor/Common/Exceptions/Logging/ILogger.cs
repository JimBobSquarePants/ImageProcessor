// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogger.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates properties and methods for logging messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Common.Exceptions
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Encapsulates properties and methods for logging messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <typeparam name="T">The type calling the logger.</typeparam>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        void Log<T>(string text, [CallerMemberName] string callerName = null);
    }
}
