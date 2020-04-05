using System;
using System.Collections.Generic;

namespace FlipLeaf
{
    public sealed class HeaderFieldDictionary : Dictionary<string, object?>
    {
        public HeaderFieldDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}
