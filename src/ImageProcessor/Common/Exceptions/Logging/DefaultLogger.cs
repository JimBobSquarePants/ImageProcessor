// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultLogger.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The default logger which logs messages to the debugger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if NET45
namespace ImageProcessor.Common.Exceptions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The default logger which logs messages to the debugger.
    /// </summary>
    public class DefaultLogger : ILogger
    {
        /// <summary>
        /// The writeline delegate.
        /// </summary>
        private readonly Action<string> debugWriteLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogger"/> class.
        /// </summary>
        public DefaultLogger()
        {
            // Runtime compile the Debug.WriteLine(string text) method so we can call it outside 
            // debug compilation.
            ParameterExpression param = Expression.Parameter(typeof(string), "text");
            MethodCallExpression caller = Expression.Call(
                       typeof(System.Diagnostics.Debug).GetRuntimeMethod(
                       "WriteLine", new[] { typeof(string) }),
                       param);

            this.debugWriteLine = Expression.Lambda<Action<string>>(caller, param)
                                            .Compile();
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <typeparam name="T">The type calling the logger.</typeparam>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        /// <param name="lineNumber">The line number where the method is called.</param>
        public void Log<T>(string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string message = string.Format("{0} - {1}: {2} {3}:{4}", DateTime.UtcNow.ToString("s"), typeof(T).Name, callerName, lineNumber, text);

            this.debugWriteLine(message);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="type">The type calling the logger.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        /// <param name="lineNumber">The line number where the method is called.</param>
        public void Log(Type type, string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string message = string.Format("{0} - {1}: {2} {3}:{4}", DateTime.UtcNow.ToString("s"), type.Name, callerName, lineNumber, text);

            this.debugWriteLine(message);
        }
    }
}
#endif