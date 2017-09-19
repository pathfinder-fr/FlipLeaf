using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlipLeaf
{
    partial class Engine
    {
        private static class Yaml
        {
            public static SiteSettings ParseConfig(string path)
            {
                var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
                using (var file = File.OpenRead(path))
                using (var reader = new StreamReader(file))
                {
                    var parser = new Parser(reader);
                    parser.Expect<StreamStart>();

                    return deserializer.Deserialize<SiteSettings>(parser);
                }
            }

            public static bool ParseHeader(ref string source, out object pageContext)
            {
                var input = new StringReader(source);
                var deserializer = new DeserializerBuilder().Build();
                var parser = new Parser(input);
                pageContext = null;


                int i;
                parser.Expect<StreamStart>();

                if (!parser.Accept<DocumentStart>())
                {
                    return false;
                }

                var doc = deserializer.Deserialize(parser);
                pageContext = ConvertDoc(doc);

                if (!parser.Accept<DocumentStart>())
                {
                    return false;
                }

                i = parser.Current.End.Index - 1;
                char c;

                do
                {
                    i++;
                    c = source[i];
                } while (c == '\r' || c == '\n');

                source = source.Substring(i);

                return true;
            }

            private static object ConvertDoc(object doc)
            {
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
}