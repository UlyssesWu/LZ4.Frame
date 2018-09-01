using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LZ4.Frame.Tests
{
    [TestClass]
    public class Lz4Test
    {
        [TestMethod]
        public void TestCompress()
        {
            var resPath = Path.Combine(Environment.CurrentDirectory, @"..\..\Res");
            var path = Path.Combine(resPath, "psb3.lz4");
            using (var fs = File.OpenRead(path))
            {
                var decoded = LZ4Frame.Decompress(fs);
                var encoded = LZ4Frame.Compress(new MemoryStream(decoded), useContentSize: false);
                var decoded2 = LZ4Frame.Decompress(new MemoryStream(encoded));
                Assert.IsTrue(decoded2.SequenceEqual(decoded));
            }
        }

        [TestMethod]
        public void TestDecompress()
        {
            var resPath = Path.Combine(Environment.CurrentDirectory, @"..\..\Res");
            var path = Path.Combine(resPath, "psb2.lz4");
            using (var fs = File.OpenRead(path))
            {
                var result = LZ4Frame.Decompress(fs);
                //File.WriteAllBytes("psb.psb", result);
            }
        }

        [TestMethod]
        public void TestDecompress2()
        {
            var resPath = Path.Combine(Environment.CurrentDirectory, @"..\..\Res");
            var path = Path.Combine(resPath, "compress.lz4");
            using (var fs = File.OpenRead(path))
            {
                var output = lz4.LZ4Helper.Decompress(File.ReadAllBytes(path));
            }
        }
    }
}
