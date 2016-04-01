
namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The type of the value in the <see cref="IFDEntry"/>. This is used to help value parsers
    /// understand the length of the data and the type of data.
    /// <example>
    /// If the <see cref="IFDEntry.ValueCount"/> property is 5 and the <see cref="IFDEntry.FieldType"/> is <see cref="Long"/>
    /// then the length of the data is 5 * sizeof(uint), and the actual data is an array of length 5 that contains uints
    /// </example>
    /// <remarks>
    /// From spec: TIFF readers should accept BYTE, SHORT, or LONG values for any unsigned
    /// integer field. This allows a single procedure to retrieve any integer value, makes
    /// reading more robust, and saves disk space in some situations.
    /// </remarks>
    /// </summary>
    public enum IFDEntryType
    {
        Invalid = 0,

        /// <summary>
        /// 8-bit unsigned integer
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes= 1)]
        Byte = 1,

        /// <summary>
        /// 8-bit byte that contains a 7-bit ASCII code; the last byte must be NUL (binary zero)
        /// Translates to a string type in .NET
        /// <remarks>
        /// From Spec: Any ASCII field can contain multiple strings, each terminated with a NUL. A
        /// single string is preferred whenever possible. The Count for multi-string fields is
        /// the number of bytes in all the strings in that field plus their terminating NUL
        /// bytes. Only one NUL is allowed between strings, so that the strings following the
        /// first string will often begin on an odd byte
        /// </remarks>
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 1)]
        AsciiString = 2,

        /// <summary>
        /// 16-bit (2-byte) unsigned inter
        /// Translates to a Int16 type in .NET
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 2)]
        Short = 3,

        /// <summary>
        /// 32-bit (4-byte) unsigned integer
        /// Translates to an Int32 type in .NET
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 4)]
        Long = 4,

        /// <summary>
        /// Two consecutive <see cref="Long"/>: the first represents the numerator of a fraction; the second, the denominator.
        /// Translates to 2 Int32 types in .NET
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 8)]
        Rational = 5,

        /// <summary>
        /// An 8-bit signed (twos-complement) integer.
        /// <remarks>https://en.wikipedia.org/wiki/Two%27s_complement</remarks>
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 1)]
        SByte = 6,

        /// <summary>
        /// An 8-bit byte that may contain anything, depending on the definition of the field
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 1)]
        Undfined = 7,

        /// <summary>
        /// A 16-bit (2-byte) signed (twos-complement) integer
        /// <remarks>https://en.wikipedia.org/wiki/Two%27s_complement</remarks>
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 2)]
        SShort = 8,

        /// <summary>
        /// A 32-bit (4-byte) signed (twos-complement) integer.
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 4)]
        SLong = 9,

        /// <summary>
        /// Two <see cref="SLong"/>: the first represents the numerator of a fraction, the second the denominator.
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 8)]
        SRational = 10,

        /// <summary>
        /// Single precision (4-byte) IEEE format.
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 4)]
        Float = 11,

        /// <summary>
        /// Double precision (8-byte) IEEE format
        /// </summary>
        [IFDEntryTypeInfo(TypeSizeInBytes = 8)]
        Double = 12,
    }
}
