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
using YamlDotNet.Serialization.NamingConventions;

namespace FlipLeaf
{
    public class Engine
    {
        private const string DefaultLayoutsFolder = "_layouts";
        private const string DefaultOutputFolder = "_site";

        private readonly string _root;

        private string _outputDir;

        public Engine(string root)
        {
            _root = root;
        }

        public SiteSettings Site { get; set; } = new SiteSettings();

        public void Init()
        {
            CompileConfig();
        }

        public void Compile(string outputDir = DefaultOutputFolder)
        {
            _outputDir = outputDir;
            RenderFolder(string.Empty);
        }

        private void CompileConfig()
        {
            var path = Path.Combine(_root, "_config.yml");
            if (!File.Exists(path))
                return;

            var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var parser = new Parser(reader);
                parser.Expect<StreamStart>();

               this.Site= deserializer.Deserialize<SiteSettings>(parser);
            }
        }

        private void RenderFolder(string directory)
        {
            var dir = Path.Combine(_root, directory);

            var targetDir = Path.Combine(_root, _outputDir, directory);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                var fileName = Path.GetFileName(file);
                var fileExtension = Path.GetExtension(file);

                if (Path.GetExtension(file) == ".md")
                {
                    var targetPath = Path.Combine(targetDir, Path.ChangeExtension(fileName, ".html"));
                    Render(Path.Combine(directory, fileName), targetPath);
                }
                else
                {
                    File.Copy(file, Path.Combine(targetDir, fileName), true);
                }
            }

            foreach (var subDir in Directory.GetDirectories(dir))
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    RenderFolder(Path.Combine(directory, subDir));
                }
                else
                {
                    var directoryName = Path.GetFileName(subDir);

                    if (string.Equals(directoryName, _outputDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (string.Equals(directoryName, DefaultLayoutsFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    RenderFolder(Path.Combine(directory, subDir));
                }
            }
        }

        public void Render(string page, string targetPath)
        {
            var source = File.ReadAllText(Path.Combine(_root, page));


            // 1) yaml
            var input = new StringReader(source);
            var deserializer = new DeserializerBuilder().Build();
            var parser = new Parser(input);


            object finalDoc = null;
            int i;
            try
            {
                parser.Expect<StreamStart>();

                if (parser.Accept<DocumentStart>())
                {
                    var doc = deserializer.Deserialize(parser);
                    finalDoc = ConvertDoc(doc);
                }

                if (!parser.Accept<DocumentStart>())
                {
                    return;
                }

                i = parser.Current.End.Index - 1;
                char c;

                do
                {
                    i++;
                    c = source[i];
                } while (c == '\r' || c == '\n');

                source = source.Substring(i);
            }
            catch (YamlException ye)
            {
                Console.WriteLine($"Unable to parse yaml header for file {page}", ye);
                return;
            }


            // 2) fluid
            if (!ViewTemplate.TryParse(source, out var template))
            {
                return;
            }

            var context = new TemplateContext();
            context.MemberAccessStrategy = new IgnoreCaseMemberAccessStrategy();
            context.MemberAccessStrategy.Register<SiteSettings>();
            context.SetValue("page", finalDoc);
            context.SetValue("site", Site);

            source = template.Render(context);

            source = Markdig.Markdown.ToHtml(source);

            var layoutFile = context.GetValue("page").GetValue("layout", context).ToStringValue();
            if (layoutFile != null)
            {
                if (Path.GetExtension(layoutFile) == "")
                    layoutFile += ".html";

                var layoutText = File.ReadAllText(Path.Combine(_root, DefaultLayoutsFolder, layoutFile));
                context.AmbientValues.Add("Body", source);
                if (!ViewTemplate.TryParse(layoutText, out var layoutTemplate))
                {
                    return;
                }

                source = layoutTemplate.Render(context);
            }

            Console.WriteLine($"Writing {targetPath}...");
            File.WriteAllText(targetPath, source);
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

        class IgnoreCaseMemberAccessStrategy: IMemberAccessStrategy        {
            private Dictionary<string, IMemberAccessor> _map = new Dictionary<string, IMemberAccessor>(StringComparer.OrdinalIgnoreCase);
            private readonly IMemberAccessStrategy _parent;

            public IgnoreCaseMemberAccessStrategy()
            {
            }

            public IgnoreCaseMemberAccessStrategy(IMemberAccessStrategy parent)
            {
                _parent = parent;
            }

            public object Get(object obj, string name)
            {
                // Look for specific property map
                if (_map.TryGetValue(Key(obj.GetType(), name), out var getter))
                {
                    return getter.Get(obj, name);
                }

                // Look for a catch-all getter
                if (_map.TryGetValue(Key(obj.GetType(), "*"), out getter))
                {
                    return getter.Get(obj, name);
                }

                return _parent?.Get(obj, name);
            }

            public void Register(Type type, string name, IMemberAccessor getter)
            {
                _map[Key(type, name)] = getter;
            }

            private string Key(Type type, string name) => $"{type.Name}.{name}";
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
