using System;
using System.Collections.Generic;
using System.Text;

namespace FlipLeaf.Services
{
    public interface ILiquidService
    {

    }

    public class LiquidService : ILiquidService
    {
        private readonly FluidLiquid.FluidParser _parser;
        public LiquidService(FlipLeafSettings settings)
        {
            _parser = new FluidLiquid.FluidParser(settings);
        }


    }
}
