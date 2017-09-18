using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace FlipLeaf
{
    public static class Engine
    {
        public static void Render(string root)
        {
            if (!Path.IsPathRooted(root))
            {
                throw new ArgumentException("root must be absolute", nameof(root));
            }

            if (root[root.Length - 1] != Path.DirectorySeparatorChar)
            {
                root += Path.DirectorySeparatorChar;
            }

            foreach (var file in Directory.GetFiles(root))
            {
                var relativeFile = file.Substring(root.Length);
                Render(root, relativeFile);
            }
        }

        public static void Render(string root, string page, string outputDir = "_site")
        {
            var text = File.ReadAllText(Path.Combine(root, page));


            // 1) yaml
            var input = new StringReader(text);
            var deserializer = new DeserializerBuilder().Build();
            var parser = new Parser(input);
            parser.Expect<StreamStart>();

            object finalDoc = null;

            if (parser.Accept<DocumentStart>())
            {
                var doc = deserializer.Deserialize(parser);
                finalDoc = ConvertDoc(doc);
            }



            if (!parser.Accept<DocumentStart>())
            {
                return;
            }

            var i = parser.Current.End.Index - 1;
            char c;

            do
            {
                i++;
                c = text[i];
            } while (c == '\r' || c == '\n');

            var source = text.Substring(i);

            // 2) fluid
            if (!ViewTemplate.TryParse(source, out var template))
            {
                return;
            }

            var context = new TemplateContext();
            context.SetValue("page", finalDoc);
            source = template.Render(context);

            source = Markdig.Markdown.ToHtml(source);

            var layoutFile = context.GetValue("page").GetValue("layout", context).ToStringValue();
            if (layoutFile != null)
            {
                var layoutText = File.ReadAllText(Path.Combine(root, layoutFile));
                context.AmbientValues.Add("Body", source);
                if (!ViewTemplate.TryParse(layoutText, out var layoutTemplate))
                {
                    return;
                }

                source = layoutTemplate.Render(context);
            }

            var fileName = Path.GetFileNameWithoutExtension(page);
            var outFile = Path.Combine(root, outputDir, fileName + ".html");
            var dir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Console.WriteLine($"Writing {outFile}...");
            File.WriteAllText(outFile, source);
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

        class ViewTemplate : BaseFluidTemplate<ViewTemplate>
        {
            static ViewTemplate()
            {
                Factory.RegisterTag<RenderBodyTag>("renderbody");
            }
        }

        public class RenderBodyTag : SimpleTag
        {
            public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
            {
                if (context.AmbientValues.TryGetValue("Body", out var body))
                {
                    await writer.WriteAsync((string)body);
                }
                else
                {
                    throw new ParseException("Could not render body, Layouts can't be evaluated directly.");
                }

                return Completion.Normal;
            }
        }
    }
}
