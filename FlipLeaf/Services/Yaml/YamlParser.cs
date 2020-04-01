using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FlipLeaf.Services.Yaml
{
    public class YamlParser
    {
        private readonly IDeserializer _deserializer;

        public YamlParser()
        {
            _deserializer = new DeserializerBuilder().Build();
        }

        public bool ParseHeader(ref string source, out Dictionary<string, object> pageContext)
        {
            var input = new StringReader(source);

            var parser = new Parser(input);
            pageContext = null;

            int i;
            parser.Consume<StreamStart>();

            if (!parser.Accept<DocumentStart>(out _))
            {
                return false;
            }

            var doc = _deserializer.Deserialize(parser);

            if (doc == null)
            {
                return false;
            }

            pageContext = ConvertDoc(doc) as Dictionary<string, object>;

            if (pageContext == null)
            {
                return false;
            }

            if (!parser.Accept<DocumentStart>(out _))
            {
                return false;
            }

            i = parser.Current.End.Index - 1;

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

            source = source.Substring(i);

            return true;
        }

        private static object ConvertDoc(object doc)
        {
            if (doc == null)
            {
                return null;
            }

            var docType = doc.GetType();

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
                    return doc;

                case TypeCode.Object:
                    if (doc == null)
                        return doc;

                    switch (doc)
                    {
                        case IDictionary<object, object> objectDict:
                            return objectDict.ToDictionary(p => p.Key.ToString(), p => ConvertDoc(p.Value));

                        case IList<object> objectList:
                            return objectList.Select(o => ConvertDoc(o)).ToList();
                        default:
                            return doc;
                    }

                default:
                    return doc;
            }
        }
    }
}
