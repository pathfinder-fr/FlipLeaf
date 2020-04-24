using System.IO;
using Xunit;

namespace FlipLeaf.Markup
{
    public class YamlMarkupTests
    {
        [Theory]
        [InlineData(@"truc chose machin #bidule")]
        [InlineData(@"")]
        public void ParseHeader_InvalidContent(string content)
        {
            var yaml = new YamlMarkup();

            var dict = yaml.ParseHeader(content, out var newcontent);

            Assert.Empty(dict);
            Assert.Equal(content, newcontent);
        }

        [Theory]
        [InlineData(@"truc chose machin #bidule")]
        [InlineData(@"")]
        public void TryParseHeader_InvalidContent(string content)
        {
            var yaml = new YamlMarkup();
            var reader = new StringReader(content);

            var parsed = yaml.TryParseHeader(reader, out var dict, out var endPosition);

            Assert.False(parsed);
            Assert.Null(dict);
            Assert.Equal(0, endPosition);
            Assert.Equal(content, reader.ReadToEnd());
        }
    }
}
