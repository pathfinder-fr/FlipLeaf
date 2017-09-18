using System;
using System.IO;

namespace FlipLeaf
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = System.Environment.CurrentDirectory;

            if (args.Length != 0)
            {
                folder = args[0];
            }

            folder = Path.GetFullPath(folder);

            Engine.Render(folder);
        }
    }
}
