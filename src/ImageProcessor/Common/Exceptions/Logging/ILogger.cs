// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;

namespace ImageProcessor
{
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
        /// <param name="lineNumber">The line number where the method is called.</param>
        void Log<T>(string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="type">The type calling the logger.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        /// <param name="lineNumber">The line number where the method is called.</param>
        void Log(Type type, string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0);
    }
}