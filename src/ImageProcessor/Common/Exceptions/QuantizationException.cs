// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuantizationException.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The exception that is thrown when quantizing an image has failed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception that is thrown when quantizing an image has failed.
    /// </summary>
    [Serializable]
    public class QuantizationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public QuantizationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public QuantizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizationException" /> class.
        /// </summary>
        public QuantizationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizationException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
        /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
        /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
        private QuantizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
