using System;
using System.IO;
using System.Linq;

namespace LZ4.Frame.CoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fs = File.OpenRead("psb.lz4"))
            {
                var decoded = LZ4Frame.Decompress(fs);
                var encoded = LZ4Frame.Compress(new MemoryStream(decoded), useContentSize: false);
                var decoded2 = LZ4Frame.Decompress(new MemoryStream(encoded));
                var r = decoded2.SequenceEqual(decoded);
                Console.WriteLine($"{r} {fs.Position}");
                Console.ReadLine();
            }
        }
    }
}
