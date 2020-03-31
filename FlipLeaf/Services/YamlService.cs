using System;
using System.Collections.Generic;
using FlipLeaf.Services.Yaml;
using Fluid;
using YamlDotNet.Core;

namespace FlipLeaf.Services
{
    public interface IYamlService
    {
        string ParseHeader(string content, out IDictionary<string, object> items);
    }

    public class YamlService : IYamlService
    {
        private readonly YamlParser _yaml;

        public YamlService()
        {
            _yaml = new YamlParser();
        }

        public string ParseHeader(string content, out IDictionary<string, object> items)
        {
            var newContent = content;
            bool parsed;
            Dictionary<string, object> pageContext;
            try
            {
                parsed = _yaml.ParseHeader(ref newContent, out pageContext);
            }
            catch (SyntaxErrorException see)
            {
                throw new ParseException($"The YAML header of the page is invalid", see);
            }

            items = new Dictionary<string, object>();

            if (parsed)
            {
                content = newContent;
                foreach (var pair in pageContext)
                {
                    items[pair.Key] = pair.Value;
                }
            }

            return content;
        }
    }
}
