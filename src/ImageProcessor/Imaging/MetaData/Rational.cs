// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rational.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents a rational number. Any number that can be expressed as the quotient or fraction p/q of two
//   numbers, p and q, with the denominator q not equal to zero.
//   <remarks>
//   Adapted from <see href="https://github.com/mckamey/exif-utils.net" /> by Stephen McKamey.
//   </remarks>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.MetaData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Represents a rational number. Any number that can be expressed as the quotient or fraction p/q of two 
    /// numbers, p and q, with the denominator q not equal to zero.
    /// <remarks>
    /// Adapted from <see href="https://github.com/mckamey/exif-utils.net"/> by Stephen McKamey.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">
    /// The type to assign to the numerator and denominator components.
    /// </typeparam>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here. Better readability.")]
    [Serializable]
    public readonly struct Rational<T> :
        IConvertible,
        IComparable,
        IComparable<T>
        where T : IConvertible
    {
        /// <summary>
        /// Represents an empty instance of <see cref="Rational{T}"/>.
        /// </summary>
        public static readonly Rational<T> Empty = new Rational<T>();

        /// <summary>
        /// The delimiter.
        /// </summary>
        private const char Delim = '/';

        /// <summary>
        /// The array containing the delimiter.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly char[] DelimSet = { Delim };

        /// <summary>
        /// The parser delegate method.
        /// </summary>
        private static ParseDelegate parser;

        /// <summary>
        /// The try parser delegate method.
        /// </summary>
        private static TryParseDelegate tryParser;

        /// <summary>
        /// The max value.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static decimal maxValue;

        /// <summary>
        /// The numerator.
        /// </summary>
        private readonly T numerator;

        /// <summary>
        /// The denominator.
        /// </summary>
        private readonly T denominator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rational{T}"/> struct. 
        /// </summary>
        /// <param name="numerator">The numerator of the rational number.</param>
        /// <param name="denominator">The denominator of the rational number.</param>
        /// <remarks>
        /// Reduces by default
        /// </remarks>
        public Rational(T numerator, T denominator)
            : this(numerator, denominator, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rational{T}"/> struct. 
        /// </summary>
        /// <param name="numerator">The numerator of the rational number.</param>
        /// <param name="denominator">The denominator of the rational number.</param>
        /// <param name="reduce">determines if should reduce by greatest common divisor</param>
        public Rational(T numerator, T denominator, bool reduce)
        {
            this.numerator = numerator;
            this.denominator = denominator;

            if (reduce)
            {
                Reduce(ref this.numerator, ref this.denominator);
            }
        }

        /// <summary>
        /// The parse delegate method.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>
        /// The <see cref="Rational{T}"/>
        /// </returns>
        private delegate T ParseDelegate(string value);

        /// <summary>
        /// The try parse delegate method.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="rational">The parsed result.</param>
        /// <returns>
        /// The <see cref="bool"/>
        /// </returns>
        private delegate bool TryParseDelegate(string value, out T rational);

        /// <summary>
        /// Gets and sets the numerator of the rational number
        /// </summary>
        public T Numerator => this.numerator;

        /// <summary>
        /// Gets and sets the denominator of the rational number
        /// </summary>
        public T Denominator => this.denominator;

        /// <summary>
        /// Gets a value indicating whether the current instance is empty.
        /// </summary>
        public bool IsEmpty => this.Equals(Empty);

        /// <summary>
        /// Gets the MaxValue
        /// </summary>
        private static decimal MaxValue
        {
            get
            {
                if (maxValue == default)
                {
                    FieldInfo max = typeof(T).GetField("MaxValue", BindingFlags.Static | BindingFlags.Public);
                    if (max != null)
                    {
                        try
                        {
                            maxValue = Convert.ToDecimal(max.GetValue(null));
                        }
                        catch (OverflowException)
                        {
                            maxValue = decimal.MaxValue;
                        }
                    }
                    else
                    {
                        maxValue = int.MaxValue;
                    }
                }

                return maxValue;
            }
        }

        /// <summary>
        /// Approximate the decimal value accurate to a precision of 0.000001m
        /// </summary>
        /// <param name="value">decimal value to approximate</param>
        /// <returns>an approximation of the value as a rational number</returns>
        /// <remarks>
        /// <see href="http://stackoverflow.com/questions/95727"/>
        /// </remarks>
        public static Rational<T> Approximate(decimal value) => Approximate(value, 0.000001m);

        /// <summary>
        /// Approximate the decimal value accurate to a certain precision
        /// </summary>
        /// <param name="value">decimal value to approximate</param>
        /// <param name="epsilon">maximum precision to converge</param>
        /// <returns>an approximation of the value as a rational number</returns>
        /// <remarks>
        /// <see href="http://stackoverflow.com/questions/95727"/>
        /// </remarks>
        public static Rational<T> Approximate(decimal value, decimal epsilon)
        {
            decimal numerator = decimal.Truncate(value);

            decimal denominator = decimal.One;

            decimal fraction = decimal.Divide(numerator, denominator);

            decimal max = MaxValue;

            while (Math.Abs(fraction - value) > epsilon && (denominator < max) && (numerator < max))
            {
                if (fraction < value)
                {
                    numerator++;
                }
                else
                {
                    denominator++;

                    decimal temp = Math.Round(decimal.Multiply(value, denominator));
                    if (temp > max)
                    {
                        denominator--;
                        break;
                    }

                    numerator = temp;
                }

                fraction = decimal.Divide(numerator, denominator);
            }

            return new Rational<T>(
                (T)Convert.ChangeType(numerator, typeof(T)),
                (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Rational{T}"/> equivalent.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The <see cref="Rational{T}"/>.
        /// </returns>
        public static Rational<T> Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Empty;
            }

            if (parser == null)
            {
                parser = BuildParser();
            }

            string[] parts = value.Split(DelimSet, 2, StringSplitOptions.RemoveEmptyEntries);
            T numerator = parser(parts[0]);
            T denominator = parts.Length > 1 ? parser(parts[1]) : default;

            return new Rational<T>(numerator, denominator);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Rational{T}"/> equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="rational">The converted <see cref="Rational{T}"/>.</param>
        /// <returns>
        /// The <see cref="Rational{T}"/>.
        /// </returns>
        public static bool TryParse(string value, out Rational<T> rational)
        {
            if (string.IsNullOrEmpty(value))
            {
                rational = Empty;
                return false;
            }

            if (tryParser == null)
            {
                tryParser = BuildTryParser();
            }

            T denominator;
            string[] parts = value.Split(DelimSet, 2, StringSplitOptions.RemoveEmptyEntries);
            if (!tryParser(parts[0], out T numerator))
            {
                rational = Empty;
                return false;
            }

            if (parts.Length > 1)
            {
                if (!tryParser(parts[1], out denominator))
                {
                    rational = Empty;
                    return false;
                }
            }
            else
            {
                denominator = default;
            }

            rational = new Rational<T>(numerator, denominator);
            return parts.Length == 2;
        }

        /// <summary>
        /// Builds a parser to convert objects.
        /// </summary>
        /// <returns>
        /// The <see cref="Rational{T}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying rational type does not support a parse method.
        /// </exception>
        /// <exception cref="TargetInvocationException">
        /// Thrown when a reflection error occurs.
        /// </exception>
        private static ParseDelegate BuildParser()
        {
            MethodInfo parse = typeof(T).GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            if (parse == null)
            {
                throw new InvalidOperationException("Underlying Rational type T must support Parse in order to parse Rational<T>.");
            }

            return value =>
                {
                    try
                    {
                        return (T)parse.Invoke(null, new object[] { value });
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw ex.InnerException;
                        }

                        throw;
                    }
                };
        }

        /// <summary>
        /// Tries to build a parser to convert objects.
        /// </summary>
        /// <returns>
        /// The <see cref="Rational{T}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying rational type does not support a parse method.
        /// </exception>
        /// <exception cref="TargetInvocationException">
        /// Thrown when a reflection error occurs.
        /// </exception>
        private static TryParseDelegate BuildTryParser()
        {
            // http://stackoverflow.com/questions/1933369
            MethodInfo tryParse = typeof(T).GetMethod(
                "TryParse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(T).MakeByRefType() },
                null);

            if (tryParse == null)
            {
                throw new InvalidOperationException("Underlying Rational type T must support TryParse in order to try-parse Rational<T>.");
            }

            return (string value, out T output) =>
                {
                    object[] args = { value, default(T) };
                    try
                    {
                        bool success = (bool)tryParse.Invoke(null, args);
                        output = (T)args[1];
                        return success;
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw ex.InnerException;
                        }

                        throw;
                    }
                };
        }

        /// <summary>
        /// Finds the greatest common divisor and reduces the fraction by this amount.
        /// </summary>
        /// <returns>the reduced rational</returns>
        public Rational<T> Reduce()
        {
            T n = this.numerator;
            T d = this.denominator;

            Reduce(ref n, ref d);

            return new Rational<T>(n, d);
        }

        /// <summary>
        /// Finds the greatest common divisor and reduces the fraction by this amount.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        private static void Reduce(ref T numerator, ref T denominator)
        {
            bool reduced = false;

            decimal n = Convert.ToDecimal(numerator);
            decimal d = Convert.ToDecimal(denominator);

            // greatest common divisor
            decimal gcd = Gcd(n, d);
            if (gcd != decimal.One && gcd != 0m)
            {
                reduced = true;
                n /= gcd;
                d /= gcd;
            }

            // cancel out signs
            if (d < 0m)
            {
                reduced = true;
                n = -n;
                d = -d;
            }

            if (reduced)
            {
                numerator = (T)Convert.ChangeType(n, typeof(T));
                denominator = (T)Convert.ChangeType(d, typeof(T));
            }
        }

        /// <summary>
        /// The least common multiple of the denominators of a set of fractions
        /// </summary>
        /// <param name="a">The first decimal.</param>
        /// <param name="b">The second decimal.</param>
        /// <returns>The lowest common denominator.</returns>
        private static decimal Lcd(decimal a, decimal b)
        {
            if (a == 0m && b == 0m)
            {
                return 0m;
            }

            return (a * b) / Gcd(a, b);
        }

        /// <summary>
        /// The largest positive decimal that divides the numbers without a remainder
        /// </summary>
        /// <param name="a">The first decimal.</param>
        /// <param name="b">The second decimal.</param>
        /// <returns>The greatest common divisor.</returns>
        private static decimal Gcd(decimal a, decimal b)
        {
            if (a < 0m)
            {
                a = -a;
            }

            if (b < 0m)
            {
                b = -b;
            }

            while (a != b)
            {
                if (a == 0m)
                {
                    return b;
                }

                if (b == 0m)
                {
                    return a;
                }

                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }

            return a;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String"/> using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> instance equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        public string ToString(IFormatProvider provider)
        {
            return string.Concat(
                this.numerator.ToString(provider),
                Delim,
                this.denominator.ToString(provider));
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal"/> number using the 
        /// specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Decimal"/> number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        public decimal ToDecimal(IFormatProvider provider)
        {
            try
            {
                decimal d = this.denominator.ToDecimal(provider);
                if (d == 0m)
                {
                    return 0m;
                }

                return this.numerator.ToDecimal(provider) / d;
            }
            catch (InvalidCastException)
            {
                long d = this.denominator.ToInt64(provider);
                if (d == 0L)
                {
                    return 0L;
                }

                return ((IConvertible)this.numerator.ToInt64(provider)).ToDecimal(provider)
                    / ((IConvertible)d).ToDecimal(provider);
            }
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using 
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A double-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        public double ToDouble(IFormatProvider provider)
        {
            double d = this.denominator.ToDouble(provider);
            if (Math.Abs(d) < 0.000001)
            {
                return 0.0;
            }

            return this.numerator.ToDouble(provider) / d;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number 
        /// using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A single-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        public float ToSingle(IFormatProvider provider)
        {
            float d = this.denominator.ToSingle(provider);
            if (Math.Abs(d) < 0.000001)
            {
                return 0.0f;
            }

            return this.numerator.ToSingle(provider) / d;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A Boolean value equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToBoolean(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that 
        /// supplies culture-specific formatting information. 
        /// </param>
        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToByte(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A Unicode character equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToChar(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToInt16(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToInt32(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToInt64(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToSByte(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToUInt16(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToUInt32(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that 
        /// supplies culture-specific formatting information. 
        /// </param>
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)this.ToDecimal(provider)).ToUInt64(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.DateTime"/> using the specified 
        /// culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DateTime"/> instance equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => new DateTime(((IConvertible)this).ToInt64(provider));

        /// <summary>
        /// Returns the <see cref="T:System.TypeCode"/> for this instance.
        /// </summary>
        /// <returns>
        /// The enumerated constant that is the <see cref="T:System.TypeCode"/> of the class or value type that 
        /// implements this interface.
        /// </returns>
        TypeCode IConvertible.GetTypeCode() => this.numerator.GetTypeCode();

        /// <summary>
        /// Converts the value of this instance to an <see cref="T:System.Object"/> of the specified 
        /// <see cref="T:System.Type"/> that has an equivalent value, using the specified culture-specific 
        /// formatting information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> instance of type <paramref name="conversionType"/> whose value is 
        /// equivalent to the value of this instance.
        /// </returns>
        /// <param name="conversionType">
        /// The <see cref="T:System.Type"/> to which the value of this instance is converted. 
        /// </param>
        /// <param name="provider">
        /// An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific 
        /// formatting information. 
        /// </param>
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            }

            Type thisType = this.GetType();
            if (thisType == conversionType)
            {
                // no conversion needed
                return this;
            }

            if (!conversionType.IsGenericType
                || typeof(Rational<>) != conversionType.GetGenericTypeDefinition())
            {
                // fall back to basic conversion
                return Convert.ChangeType(this, conversionType, provider);
            }

            // auto-convert between Rational<T> types by converting Numerator/Denominator
            Type genericArg = conversionType.GetGenericArguments()[0];
            object[] ctorArgs =
            {
                Convert.ChangeType(this.Numerator, genericArg, provider),
                Convert.ChangeType(this.Denominator, genericArg, provider)
            };

            ConstructorInfo ctor = conversionType.GetConstructor(new[] { genericArg, genericArg });
            if (ctor == null)
            {
                throw new InvalidCastException("Unable to find constructor for Rational<" + genericArg.Name + ">.");
            }

            return ctor.Invoke(ctorArgs);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the 
        /// other object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has these meanings: Value Meaning Less than zero This instance precedes 
        /// <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order 
        /// as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="obj"/> is not the same type as this instance. 
        /// </exception>
        public int CompareTo(object obj)
        {
            if (obj is Rational<T> rational)
            {
                // Differentiate between a real zero and a divide by zero
                // work around divide by zero value to get meaningful comparisons
                var other = rational;
                if (Convert.ToDecimal(this.denominator) == 0m)
                {
                    if (Convert.ToDecimal(other.denominator) == 0m)
                    {
                        return Convert.ToDecimal(this.numerator).CompareTo(Convert.ToDecimal(other.numerator));
                    }

                    if (Convert.ToDecimal(other.numerator) == 0m)
                    {
                        return Convert.ToDecimal(this.denominator).CompareTo(Convert.ToDecimal(other.denominator));
                    }
                }
                else if (Convert.ToDecimal(other.denominator) == 0m)
                {
                    if (Convert.ToDecimal(this.numerator) == 0m)
                    {
                        return Convert.ToDecimal(this.denominator).CompareTo(Convert.ToDecimal(other.denominator));
                    }
                }
            }

            return Convert.ToDecimal(this).CompareTo(Convert.ToDecimal(obj));
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the 
        /// other object. 
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these 
        /// meanings: Value Meaning Less than zero This instance precedes <paramref name="other"/> in the sort order.  
        /// Zero This instance occurs in the same position in the sort order as <paramref name="other"/>. Greater than 
        /// zero This instance follows <paramref name="other"/> in the sort order. 
        /// </returns>
        /// <param name="other">An object to compare with this instance. </param>
        public int CompareTo(T other) => decimal.Compare(Convert.ToDecimal(this), Convert.ToDecimal(other));

        /// <summary>
        /// Performs a numeric negation of the operand.
        /// </summary>
        /// <param name="rational">The rational to negate.</param>
        /// <returns>
        /// The negated rational.
        /// </returns>
        public static Rational<T> operator -(Rational<T> rational)
        {
            var numerator = (T)Convert.ChangeType(-Convert.ToDecimal(rational.numerator), typeof(T));
            return new Rational<T>(numerator, rational.denominator);
        }

        /// <summary>
        /// Computes the sum of two rational instances.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed sum.</returns>
        public static Rational<T> operator +(Rational<T> r1, Rational<T> r2)
        {
            decimal n1 = Convert.ToDecimal(r1.numerator);
            decimal d1 = Convert.ToDecimal(r1.denominator);
            decimal n2 = Convert.ToDecimal(r2.numerator);
            decimal d2 = Convert.ToDecimal(r2.denominator);

            decimal denominator = Lcd(d1, d2);
            if (denominator > d1)
            {
                n1 *= denominator / d1;
            }

            if (denominator > d2)
            {
                n2 *= denominator / d2;
            }

            decimal numerator = n1 + n2;

            return new Rational<T>((T)Convert.ChangeType(numerator, typeof(T)), (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Computes the subtraction of one rational instance from another.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static Rational<T> operator -(Rational<T> r1, Rational<T> r2) => r1 + (-r2);

        /// <summary>
        /// Computes the product of multiplying two rational instances.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed product.</returns>
        public static Rational<T> operator *(Rational<T> r1, Rational<T> r2)
        {
            decimal numerator = Convert.ToDecimal(r1.numerator) * Convert.ToDecimal(r2.numerator);
            decimal denominator = Convert.ToDecimal(r1.denominator) * Convert.ToDecimal(r2.denominator);

            return new Rational<T>((T)Convert.ChangeType(numerator, typeof(T)), (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Computes the product of dividing two rational instances.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed product.</returns>
        public static Rational<T> operator /(Rational<T> r1, Rational<T> r2) => r1 * new Rational<T>(r2.denominator, r2.numerator);

        /// <summary>
        /// Determines whether the first rational operand is less than the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator <(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) < 0;

        /// <summary>
        /// Determines whether the first rational operand is less than or equal to the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator <=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) <= 0;

        /// <summary>
        /// Determines whether the first rational operand is greater than the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator >(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) > 0;

        /// <summary>
        /// Determines whether the first rational operand is greater than or equal to the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator >=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) >= 0;

        /// <summary>
        /// Determines whether the first rational operand is equal to the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator ==(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) == 0;

        /// <summary>
        /// Determines whether the first rational operand is not equal to the second.
        /// </summary>
        /// <param name="r1">The first rational operand.</param>
        /// <param name="r2">The second rational operand.</param>
        /// <returns>The computed result.</returns>
        public static bool operator !=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) != 0;

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString() => Convert.ToString(this, CultureInfo.InvariantCulture);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => this.CompareTo(obj) == 0;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Numerator, this.Denominator).GetHashCode();
    }
}
