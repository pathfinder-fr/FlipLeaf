using System.Collections.Generic;

namespace FlipLeaf.Services
{
    public interface ILiquidService
    {
        string Parse(string content, IDictionary<string, object> pageContext);
    }

    public class LiquidService : ILiquidService
    {
        private readonly FluidLiquid.FluidParser _parser;

        public LiquidService(FlipLeafSettings settings)
        {
            _parser = new FluidLiquid.FluidParser(settings);
        }

        public string Parse(string content, IDictionary<string, object> pageContext)
        {
            var template = _parser.ParseTemplate(content);
            var templateContext = _parser.PrepareContext(pageContext);
            return _parser.Parse(template, templateContext);
        }
    }
}
