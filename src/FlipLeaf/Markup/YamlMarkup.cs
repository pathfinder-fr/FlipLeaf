using Microsoft.Extensions.Primitives;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FlipLeaf.Markup
{
    public interface IYamlMarkup
    {
        void WriteHeaderValue(TextWriter writer, string name, StringValues values, string? defaultValue);

        HeaderFieldDictionary ParseHeader(string content, out string newContent);

        bool TryParseHeader(TextReader input, out HeaderFieldDictionary? pageContext, out int endPosition);
    }

    public class YamlMarkup : IYamlMarkup
    {
        private readonly IDeserializer _deserializer;

        public YamlMarkup()
        {
            _deserializer = new DeserializerBuilder().Build();
        }

        public void WriteHeaderValue(TextWriter writer, string name, StringValues values, string? defaultValue)
        {
            if (values == StringValues.Empty && defaultValue == null)
            {
                return;
            }

            writer.Write(name);
            writer.Write(": ");

            if (values == StringValues.Empty)
            {
                if (defaultValue != null)
                {
                    // default value
                    WriteValue(writer, defaultValue);
                }
            }
            else if (values.Count > 1)
            {
                // mutiple values
                writer.WriteLine();

                foreach (var value in values)
                {
                    writer.Write("  - ");
                    WriteValue(writer, value ?? string.Empty);
                }
            }
            else
            {
                // single value
                WriteValue(writer, values[0] ?? string.Empty);
            }

            static void WriteValue(TextWriter w, string value)
            {
                if (value == string.Empty)
                {
                    w.Write("''");
                    w.WriteLine();
                }
                else if (value.IndexOf(',') != -1)
                {
                    w.Write("\"");
                    w.Write(value);
                    w.Write("\"");
                    w.WriteLine();
                }
                else
                {
                    w.Write(value);
                    w.WriteLine();
                }
            }
        }

        public HeaderFieldDictionary ParseHeader(string content, out string newContent)
        {
            HeaderFieldDictionary items;
            newContent = content;
            bool parsed;
            HeaderFieldDictionary? pageContext;
            try
            {
                parsed = this.TryParseHeader(ref newContent, out pageContext);
            }
            catch (SyntaxErrorException see)
            {
                throw new InvalidOperationException($"The YAML header of the page is invalid", see);
            }

            items = new HeaderFieldDictionary();

            if (parsed && pageContext != null)
            {
                foreach (var pair in pageContext)
                {
                    items[pair.Key] = pair.Value;
                }
            }

            return items;
        }

        public bool TryParseHeader(ref string source, out HeaderFieldDictionary? pageContext)
        {
            pageContext = null;
            if (!source.StartsWith("---", StringComparison.Ordinal))
            {
                return false;
            }

            using var input = new StringReader(source);

            if (!TryParseHeader(input, out pageContext, out var i))
            {
                return false;
            }

            // skip content
            char c;
            do
            {
                i++;

                if (i >= source.Length)
                {
                    source = string.Empty;
                    return true;
                }

                c = source[i];
            } while (c == '\r' || c == '\n');

            source = source[i..];

            return true;
        }

        public bool TryParseHeader(TextReader input, out HeaderFieldDictionary? pageContext, out int endPosition)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var parser = new Parser(input);

            pageContext = null;
            endPosition = 0;

            if (!parser.Accept<StreamStart>(out _))
            {
                return false;
            }

            parser.Consume<StreamStart>();

            if (!parser.Accept<DocumentStart>(out var docStart))
            {
                return false;
            }

            // we don't accept implicit start document: the --- are mandatory
            // they serve as a method to detect the yaml header
            if (docStart.IsImplicit)
            {
                return false;
            }

            var doc = _deserializer.Deserialize(parser);
            if (doc == null)
            {
                return false;
            }

            pageContext = Convert(doc) as HeaderFieldDictionary;
            if (pageContext == null)
            {
                return false;
            }

            if (!parser.Accept<DocumentStart>(out _) || parser.Current == null)
            {
                return false;
            }

            endPosition = parser.Current.End.Index - 1;

            return true;

        }

        private static object? Convert(object value)
        {
            if (value == null)
            {
                return null;
            }

            var docType = value.GetType();

            switch (Type.GetTypeCode(docType))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Empty:
                    return value;

                case TypeCode.Object:
                    if (value == null)
                        return value;

                    switch (value)
                    {
                        case IDictionary<object, object> objectDict:
                            var result = new HeaderFieldDictionary();

                            foreach (var pair in objectDict)
                            {
                                var pairKey = pair.Key?.ToString() ?? string.Empty;
                                var pairValue = Convert(pair.Value);
                                result[pairKey] = pairValue;
                            }

                            return result;

                        case IList<object> objectList:
                            return objectList.Select(o => Convert(o)).ToList();

                        default:
                            return value;
                    }

                default:
                    return value;
            }
        }
    }
}
