using System;

namespace FlipLeaf
{
    class Program
    {
        static void Main(string[] args)
        {
            var output = Engine.Render(@"C:\Projets\Perso\FlipLeaf\sample", "sample.md");

            Console.WriteLine(output);
        }
    }
}
