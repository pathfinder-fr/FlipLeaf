using System;
using System.Collections.Generic;
using System.Text;

namespace FlipLeaf.Services.Git
{
    public class Commit
    {
        public string Sha { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Authored { get; set; }
    }
}
