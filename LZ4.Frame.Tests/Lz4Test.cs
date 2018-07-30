using System;
using System.IO;
using System.Linq;
using LZ4.Frame;
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
            var path = Path.Combine(resPath, "psb.lz4");
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
            var path = Path.Combine(resPath, "psb.lz4");
            using (var fs = File.OpenRead(path))
            {
                var result = LZ4Frame.Decompress(fs);
            }
        }
    }
}
