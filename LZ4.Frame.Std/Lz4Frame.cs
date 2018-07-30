using System;
using System.Diagnostics;
using System.IO;
using NeoSmart.Hashing.XXHash.Core;

namespace LZ4.Frame
{
    //https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md

    public enum LZ4MaxBlockSize
    {
        Auto = 0,
        KB64 = 4,
        KB256 = 5,
        MB1 = 6,
        MB4 = 7,
    }

    public class LZ4Frame
    {
        public const int MAGIC = 0x184D2204;

        internal static readonly int[] BlockMaximum = new[]
            {0, 0, 0, 0, 64 * 1024, 256 * 1024, 1024 * 1024, 4 * 1024 * 1024};

        /// <summary>
        /// LZ4 Frame Compress
        /// </summary>
        /// <param name="input">stream to be compressed</param>
        /// <param name="blockSizeType">max size (before compress) per block</param>
        /// <param name="useIndependenceBlock">only True works</param>
        /// <param name="useUncompressedBlock">If a block is better not to compress, will use original data</param>
        /// <param name="useBlockChecksum">Checksum for every block, not recommended</param>
        /// <param name="useContentChecksum">Checksum for whole content, recommended</param>
        /// <param name="useContentSize">If content size can be get, there's no reason to set it to False</param>
        /// <returns></returns>
        public static byte[] Compress(Stream input, LZ4MaxBlockSize blockSizeType = LZ4MaxBlockSize.Auto, bool useIndependenceBlock = true, bool useUncompressedBlock = true, bool useBlockChecksum = false, bool useContentChecksum = true, bool useContentSize = true)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                uint contentChecksum = 0;
                byte[] result = null;
                int originalLength = (int)input.Length;
                int len = originalLength;
                input.Seek(0, SeekOrigin.Begin);
                if (useContentChecksum)
                {
                    contentChecksum = XXHash.XXH32(input);
                }
                input.Seek(0, SeekOrigin.Begin);
                BinaryReader br = new BinaryReader(input);

                //if (!useIndependenceBlock) //Incorrect
                //{
                //    result = LZ4Codec.Encode(br.ReadBytes(originalLength), 0, originalLength);
                //    len = result.Length;
                //    br = new BinaryReader(new MemoryStream(result));
                //}

                if (blockSizeType == LZ4MaxBlockSize.Auto)
                {
                    //if (!useContentSize)
                    //{
                    //    blockSize = LZ4MaxBlockSize.KB256;
                    //}
                    //Infer block size
                    if (len > BlockMaximum[(int)LZ4MaxBlockSize.MB4])
                    {
                        blockSizeType = LZ4MaxBlockSize.MB4;
                    }
                    else if (len > BlockMaximum[(int)LZ4MaxBlockSize.MB1])
                    {
                        blockSizeType = LZ4MaxBlockSize.MB1;
                    }
                    else if (len > BlockMaximum[(int)LZ4MaxBlockSize.KB256])
                    {
                        blockSizeType = LZ4MaxBlockSize.KB256;
                    }
                    else
                    {
                        blockSizeType = LZ4MaxBlockSize.KB64;
                    }
                }
                //Magic
                bw.Write(MAGIC);
                // -Flags (1 byte)
                // -Version Number (bit 7 and 6 must be 01)
                // -Block Independence flag (bit 5, 1 for independent and 0 for part of a chain of blocks) - only implemented 1
                // -Block Checksum flag (bit 4, on / off)
                // -Content Size flag (bit 3, on / off) - see no reason for not provided that so please use 1
                // -Content checksum flag (bit 2, on / off)
                // -Reserved bit (bit 1, must be 0)
                // -DictID flag (bit 0, on / off) - not implemented so forced 0
                byte flags = 0b01_1_0_0_0_0_0;
                //if (useIndependenceBlock)
                //{
                //    flags |= 0b00_1_0_0_0_0_0;
                //}
                if (useBlockChecksum)
                {
                    flags |= 0b00_0_1_0_0_0_0;
                }
                if (useContentSize)
                {
                    flags |= 0b00_0_0_1_0_0_0;
                }
                if (useContentChecksum)
                {
                    flags |= 0b00_0_0_0_1_0_0;
                }

                bw.Write(flags);

                // BlockDescriptor (1 byte)
                // Reserved bit (bit 7, must be 0)
                // Block Max Size (bit 6-5-4, 111 means 4 MB)
                // Reserved bits (bit 3-2-1-0, must be 0)
                byte bd = 0b0_101_0000;
                var blockMaxSize = BlockMaximum[(int)blockSizeType];
                switch (blockSizeType)
                {
                    case LZ4MaxBlockSize.KB64: //4
                        bd = 0b0_100_0000;
                        break;
                    case LZ4MaxBlockSize.MB1: //6
                        bd = 0b0_110_0000;
                        break;
                    case LZ4MaxBlockSize.MB4: //7
                        bd = 0b0_111_0000;
                        break;
                    case LZ4MaxBlockSize.KB256: //5
                    default:
                        break;
                }
                bw.Write(bd);

                var headerSize = 2;
                if (useContentSize)
                {
                    bw.Write(input.Length); //long
                    headerSize += 8;
                }
                //DictID, ignored

                var header = new byte[headerSize];
                bw.BaseStream.Seek(-headerSize, SeekOrigin.Current);
                bw.BaseStream.Read(header, 0, headerSize);
                // Header Checksum - Checksum on the combined Frame Descriptor (1 byte)
                // Get 2nd byte of XXHash32 - xxh32() : (xxh32()>>8) & 0xFF
                byte headerChecksum = (byte)((XXHash.XXH32(header) >> 8) & 0xFF);
                bw.Write(headerChecksum);

                while (true)
                {
                    //var pos = bw.BaseStream.Position;
                    var remain = (int)(br.BaseStream.Length - br.BaseStream.Position);
                    if (remain == 0)
                    {
                        break;
                    }
                    var shouldEnd = remain <= blockMaxSize;
                    var blockOriSize = shouldEnd ? remain : blockMaxSize;
                    if (useIndependenceBlock)
                    {
                        var blockData = br.ReadBytes(blockOriSize);
                        var compressed = LZ4Codec.Encode(blockData, 0, blockData.Length);
                        var blockSize = compressed.Length;
                        if (compressed.Length > blockData.Length && useUncompressedBlock)
                        {
                            blockSize = blockData.Length;
                            blockSize |= 1 << 31;
                            bw.Write(blockSize);
                            bw.Write(blockData);
                        }
                        else
                        {
                            bw.Write(blockSize);
                            bw.Write(compressed);
                        }

                        if (useBlockChecksum)
                        {
                            bw.Write(XXHash.XXH32(blockData));
                        }
                    }
                    //else //don't use independent blocks, not implemented
                    //{
                    //    blockOriSize |= 0 << 31;
                    //    bw.Write(blockOriSize);
                    //    bw.Write(br.ReadBytes(blockOriSize));
                    //    //Block Checksum unsupported
                    //}

                    if (shouldEnd)
                    {
                        break;
                    }
                }

                //End mark
                bw.Write((uint)0);
                if (useContentChecksum)
                {
                    bw.Write(contentChecksum);
                }
                br.Dispose();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// LZ4 Frame Decompress
        /// </summary>
        /// <param name="input">stream to be decompressed</param>
        /// <returns></returns>
        public static byte[] Decompress(Stream input)
        {
            BinaryReader br = new BinaryReader(input);

            //Magic
            if (br.ReadInt32() != MAGIC)
            {
                throw new FormatException("Magic number incorrect");
            }

            //Frame Descriptor (3-15 bytes)
            var fdLength = 2;
            // Parse flags
            var flg = br.ReadByte();
            bool version = ((flg | 0b00000001) >> 6) == 1;
            bool blockIndependenceFlag = ((flg & 0b00100000) >> 5) == 1;
            bool blockChecksumFlag = ((flg & 0b00010000) >> 4) == 1;
            bool contentSizeFLag = ((flg & 0b00001000) >> 3) == 1;
            bool contentChecksumFlag = ((flg & 0b00000100) >> 2) == 1;
            //bool reserved = ((fdFlag & 0b00000010) >> 1) == 1;
            bool dictionaryIdFlag = ((flg & 0b00000001)) == 1;
            // Parse block descriptor
            var bd = br.ReadByte();
            int blockMaxSizeFlag = ((bd >> 4) & 0b0111);
            int blockMaxSize = BlockMaximum[blockMaxSizeFlag];
            // Parse content size
            ulong contentSize = 0;
            if (contentSizeFLag)
            {
                fdLength += 8;
                contentSize = br.ReadUInt64();
                Debug.WriteLine("Expected content size: " + contentSize);
            }

            // Parse dictionary ID
            int dictionaryId = 0;
            if (dictionaryIdFlag)
            {
                fdLength += 4;
                dictionaryId = br.ReadInt32();
            }

            // Parse Header Checksum
            //var pos = br.BaseStream.Position;
            br.BaseStream.Seek(-fdLength, SeekOrigin.Current);
            var fdCheck = br.ReadBytes(fdLength);
            var hc = br.ReadByte();

            // Calculate checksum with parsed header values
            var calculatedHeaderChecksum = (XXHash.XXH32(fdCheck) >> 8) & 0xFF;
            if (hc != calculatedHeaderChecksum)
            {
                throw new FormatException("Header doesn't match checksum");
            }

            //var blockCount = 0;
            var output = new byte[contentSize];
            int outputOffset = 0;
            //Block
            while (true)
            {
                //pos = br.BaseStream.Position;
                var blockSize = br.ReadInt32();
                if (blockSize == 0)
                {
                    break;
                }

                var dataIsUncompressed = false;
                //blockCount++;
                //var binary = Convert.ToString(blockSize, 2);
                // Check if the data is compressed by inspecting the highest bit - 0 for compressed, 1 for uncompressed
                if ((blockSize >> 31) == 1)
                {
                    // uncompressed
                    dataIsUncompressed = true;
                    // flip the bit back
                    blockSize &= ~(1 << 31);
                }

                var sizeRemain = output.Length - outputOffset;
                var blockData = br.ReadBytes(blockSize);
                if (dataIsUncompressed)
                {
                    Array.Copy(blockData, 0, output, outputOffset, blockData.Length);
                }
                else
                {
                    var outputLen = LZ4Codec.Decode(blockData, 0, blockData.Length, output, outputOffset,
                        blockMaxSize > sizeRemain ? sizeRemain : blockMaxSize);
                    outputOffset += outputLen;
                }

                if (blockChecksumFlag)
                {
                    if (br.ReadUInt32() != XXHash.XXH32(blockData))
                    {
                        Debug.WriteLine("Block Checksum incorrect");
                    }
                }
            }

            var end = br.ReadUInt32();
            // Content Checksum
            if (contentChecksumFlag)
            {
                var cc = end != 0 ? end : br.ReadUInt32();
                if (XXHash.XXH32(output) != cc)
                {
                    Debug.WriteLine("Block Checksum incorrect");
                }
            }
            return output;
        }
    }
}
