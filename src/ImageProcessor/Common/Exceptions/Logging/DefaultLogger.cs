// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultLogger.cs" company="James South">
//   Copyright (c) James South.
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
        private Action<string> debugWriteLine;

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
        public void Log<T>(string text, [CallerMemberName] string callerName = null)
        {
            string message = string.Format("{0} : {1} {2}", typeof(T).Name, callerName, text);

            this.debugWriteLine(message);
        }
    }
}
#endif