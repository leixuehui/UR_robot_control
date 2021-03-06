using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toml
{
    public static class TypeParsers
    {
        /// <summary>
        /// Represents the base ParseResult instance.
        /// </summary>
        private class ParseResultBase
        {
            /// <summary>
            /// Initializes a new instance of the ParseResultBase class.
            /// </summary>
            /// <param name="error">The exception that occurred while parsing the value.</param>
            public ParseResultBase(Exception error)
            {
                this.Success = false;
                this.Error = error;
            }

            /// <summary>
            /// Initializes a new instance of the ParseResultBase class.
            /// </summary>
            public ParseResultBase()
            {
                this.Success = true;
            }

            /// <summary>
            /// Indicates whether or not the parsing succeeded.
            /// </summary>
            public bool Success { get; private set; }

            /// <summary>
            /// Gets the exception that occurred while parsing the value.
            /// </summary>
            public Exception Error { get; private set; }
        }

        /// <summary>
        /// Represents the result of a parse attempt.
        /// </summary>
        /// <typeparam name="T">The type of object being parsed.</typeparam>
        private class ParseResult<T> : ParseResultBase
        {
            /// <summary>
            /// The parsed value, if any.
            /// </summary>
            private T _value;

            /// <summary>
            /// Initializes a new instance of the ParseResult class.
            /// </summary>
            /// <param name="error">The exception that occurred while parsing the value.</param>
            public ParseResult(Exception error)
                : base(error)
            {
                this.Value = default(T);
            }

            /// <summary>
            /// Initializes a new instance of the ParseResult class.
            /// </summary>
            /// <param name="value">The parsed value.</param>
            public ParseResult(T value)
                : base()
            {
                this.Value = value;
            }

            /// <summary>
            /// Gets the value generated by the parser
            /// </summary>
            public T Value
            {
                get
                {
                    if (!this.Success)
                    {
                        throw this.Error;
                    }

                    return _value;
                }
                private set
                {
                    _value = value;
                }
            }
        }

        /// <summary>
        /// Stores the registered type parsers.
        /// </summary>
        private static Dictionary<Type, Func<string, ParseResultBase>> ParserFuncs = RegisterParsers();

        /// <summary>
        /// Registers the supported type parsers.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, Func<string, ParseResultBase>> RegisterParsers()
        {
            var parsers = new Dictionary<Type, Func<string, ParseResultBase>>();

            parsers.Add(typeof(Int32), ParseInt32);
            parsers.Add(typeof(Int64), ParseInt64);
            parsers.Add(typeof(UInt32), ParseUInt32);
            parsers.Add(typeof(UInt64), ParseUInt64);
            parsers.Add(typeof(float), ParseFloat);
            parsers.Add(typeof(double), ParseDouble);
            parsers.Add(typeof(DateTime), ParseDateTime);
            parsers.Add(typeof(string), ParseString);

            return parsers;
        }

        #region Parse Funcs

        /// <summary>
        /// Parses an Int32 value.
        /// </summary>
        private static ParseResult<Int32> ParseInt32(string value)
        {
            try
            {
                return new ParseResult<Int32>(Int32.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<Int32>(ex);
            }
        }

        /// <summary>
        /// Parses an Int64 value.
        /// </summary>
        private static ParseResult<Int64> ParseInt64(string value)
        {
            try
            {
                return new ParseResult<Int64>(Int64.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<Int64>(ex);
            }
        }

        /// <summary>
        /// Parses a UInt32 value.
        /// </summary>
        private static ParseResult<UInt32> ParseUInt32(string value)
        {
            try
            {
                return new ParseResult<UInt32>(UInt32.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<UInt32>(ex);
            }
        }

        /// <summary>
        /// Parses an UInt64 value.
        /// </summary>
        private static ParseResult<UInt64> ParseUInt64(string value)
        {
            try
            {
                return new ParseResult<UInt64>(UInt64.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<UInt64>(ex);
            }
        }

        /// <summary>
        /// Parses a float value.
        /// </summary>
        private static ParseResult<float> ParseFloat(string value)
        {
            try
            {
                return new ParseResult<float>(float.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<float>(ex);
            }
        }

        /// <summary>
        /// Parses an double value.
        /// </summary>
        private static ParseResult<double> ParseDouble(string value)
        {
            try
            {
                return new ParseResult<double>(double.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<double>(ex);
            }
        }

        /// <summary>
        /// Parses a DateTime value.
        /// </summary>
        private static ParseResult<DateTime> ParseDateTime(string value)
        {
            try
            {
                return new ParseResult<DateTime>(DateTime.Parse(value));
            }
            catch (Exception ex)
            {
                return new ParseResult<DateTime>(ex);
            }
        }

        /// <summary>
        /// Parses a string value.
        /// </summary>
        private static ParseResult<string> ParseString(string value)
        {
            return new ParseResult<string>(value);
        }

        #endregion

        /// <summary>
        /// Attempts to parse the given value into the requested type.
        /// </summary>
        public static bool TryParse<T>(string value, out T result)
        {
            result = default(T);

            Func<string, ParseResultBase> parser = null;
            if (!TypeParsers.ParserFuncs.TryGetValue(typeof(T), out parser))
            {
                return false;
            }

            var parseResult = parser(value);
            if (parseResult is ParseResult<T>)
            {
                if (parseResult.Success)
                {
                    result = ((ParseResult<T>)parseResult).Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse the specified value into the requested type.
        /// </summary>
        public static T Parse<T>(string value)
        {
            Func<string, ParseResultBase> parser = null;
            if (TypeParsers.ParserFuncs.TryGetValue(typeof(T), out parser))
            {
                return ((ParseResult<T>)parser(value)).Value;
            }

            if (typeof(T) == typeof(object))
            {
                return (T)((object)value);
            }

            throw new InvalidOperationException("A parser for the specified type is not registered");
        }

        // TODO: MAKE THIS WORK
        //public static object Parse(Type type, string value)
        //{
        //    Func<string, ParseResultBase> parser = null;
        //    if (TypeParsers.ParserFuncs.TryGetValue(type, out parser))
        //    {
        //        var parserResult = typeof(ParseResult<>).MakeGenericType(type);
        //        return ((parserResult)parser(value)).Value;
        //    }

        //    if (typeof(T) == typeof(object))
        //    {
        //        return (T)((object)value);
        //    }

        //    throw new InvalidOperationException("A parser for the specified type is not registered");
        //}
    }
}
