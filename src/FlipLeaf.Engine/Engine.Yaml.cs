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
        }
    }
}