using LitJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;

namespace Rdc
{
    public unsafe class Section : ISerializable
    {
        public const int LZ4_BLOCK_SIZE = 64 * 1024; // lz4io.cpp: static const uint64_t lz4BlockSize = 64 * 1024;
        /// <summary>
        /// 用于查找 Font Texture 的 SetResourceName chunk 的 magic data, 其实是 长度 + 字符串"Font Texture"
        /// </summary>
        public static readonly byte[] fontTextureMagicData = new byte[16] { 0x0c, 0x00, 0x00, 0x00, 0x46, 0x6f, 0x6e, 0x74, 0x20, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65 };

        public ChunkManager chunkManager { get; private set; }

        public BinarySectionHeader header;
        public SectionLocation location;

        public SectionProperties props;
        public ExtThumbnailHeader thumbHeader;

        public List<ChunkMeta> chunkMetas { get; private set; } // 仅 FrameCapture 类型的 section 存在

        public byte[] diskData { get; private set; }
        public byte[] uncompressedData { get; private set; }
        public byte[] thumbPixels { get; private set; }

        private ChunkMeta.UpdateSubresourceChunkInfo fontTextureChunk;

        public void LoadFromJson(JsonData jsonData, StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            header = new BinarySectionHeader();
            props = new SectionProperties();
            location = new SectionLocation();

            header.LoadFromStream(br);

            {
                props.flags = header.sectionFlags;
                props.type = header.sectionType;
                props.compressedSize = header.sectionCompressedLength;
                props.uncompressedSize = header.sectionUncompressedLength;
                props.version = header.sectionVersion;
                props.name = header.name;
            }

            {
                location.headerOffset = (ulong)offset;
                location.dataOffset = (ulong)br.BaseStream.Position;
                location.diskLength = header.sectionCompressedLength;
            }

            diskData = new byte[header.sectionCompressedLength];
            br.Read(diskData, 0, diskData.Length);

            UnCompressData();

            if (header.sectionType == SectionType.ExtendedThumbnail)
            {
                using (var rawStream = new MemoryStream(uncompressedData))
                using (var thumbBr = new BinaryReader(rawStream))
                {
                    thumbHeader = new ExtThumbnailHeader();
                    thumbHeader.LoadFromStream(thumbBr);

                    thumbPixels = new byte[thumbHeader.len];
                    thumbBr.Read(thumbPixels, 0, thumbPixels.Length);
                }
            }

            ProcessChunks();

#if DEBUG
            if (props.type == SectionType.ExtendedThumbnail)
            {
                File.WriteAllBytes("D:/thumb.png", thumbPixels);
            }
            else
            {
                File.WriteAllBytes($"D:/{props.name.Replace('/', '_')}_lz4.bin", diskData);
                File.WriteAllBytes($"D:/{props.name.Replace('/', '_')}_raw.bin", uncompressedData);
            }

            if(chunkMetas != null)
            {
                string chunkInfos = DumpChunkInfos();
                File.WriteAllText("D:/chunkMetas.txt", chunkInfos);
            }
#endif

            return (int)(br.BaseStream.Position - offset);
        }

        public void ProcessChunks()
        {
            // 当前仅处理 Font Texture chunk
            if (props.type != SectionType.FrameCapture)
                return;

            chunkMetas = new List<ChunkMeta>();

            using(MemoryStream ms = new MemoryStream(uncompressedData))
            using(BinaryReader br = new BinaryReader(ms))
            {
                int index = 1; // RenderDoc的chunk从1开始计数
                while(ms.Position < uncompressedData.Length)
                {
                    var chunkMeta = new ChunkMeta(index++);
                    chunkMeta.LoadFromStream(br);

                    chunkMetas.Add(chunkMeta);
                }
            }

            chunkManager = new ChunkManager();
            chunkManager.LoadChunksFromSection(this);

            //// 通过关键字查找chunk offset
            //int fontNameOffset = uncompressedData.IndexOf(fontTextureMagicData);
            //if (fontNameOffset == -1)
            //{
            //    Console.WriteLine("can not find [Font Texture] chunk");
            //    return;
            //}

            //using (MemoryStream ms = new MemoryStream(uncompressedData))
            //using (BinaryReader br = new BinaryReader(ms))
            //{
            //    int SetResourceNameOffset = fontNameOffset + fontTextureMagicData.Length;
            //    SetResourceNameOffset = (SetResourceNameOffset + 63) & (~63);

            //    ms.Position = SetResourceNameOffset;

            //    fontTextureChunk = new ChunkMeta();
            //    fontTextureChunk.LoadFromStream(br);
            //}
        }

        public void SaveToJson(StringBuilder sb, JsonData jsonData)
        {
            throw new NotImplementedException();
        }

        public int SaveToStream(BinaryWriter bw)
        {
            // 将数据保存为未压缩格式

            long offset = bw.BaseStream.Position;

            header.SetToUncompressFormat();
            header.SaveToStream(bw);
            bw.Write(uncompressedData, 0, uncompressedData.Length);

            return (int)(bw.BaseStream.Position - offset);
        }

        public void UnCompressData()
        {
            uncompressedData = null;
            MemoryStream rawStream;

            if ((header.sectionFlags & SectionFlags.LZ4Compressed) == SectionFlags.LZ4Compressed)
            {
                rawStream = new MemoryStream();

                int outBuffLen = LZ4Decoder.LZ4_COMPRESSBOUND(LZ4_BLOCK_SIZE);
                var outBuffer0 = new byte[outBuffLen];
                var outBuffer1 = new byte[outBuffLen];

                fixed (void* pDiskBase = diskData)
                fixed (void* pOutBuff0 = outBuffer0)
                fixed (void* pOutBuff1 = outBuffer1)
                {
                    byte* pDiskData = (byte*)pDiskBase;
                    byte* pOutBuffer = (byte*)pOutBuff0;

                    using (LZ4Decoder decoder = new LZ4Decoder())
                    {
                        int loopNo = 0;
                        while (true)
                        {
                            if ((pDiskData - (byte*)pDiskBase) >= diskData.Length)
                                break;

                            /*
                             * LZ4 continue 解压会使用上一次解压出的数据的作为下一次解压的字典并且保存数据指针，因此需要不停交换缓冲区
                             */
                            bool useBuff0 = loopNo % 2 == 0;
                            pOutBuffer = (byte *)(useBuff0 ? pOutBuff0 : pOutBuff1);

                            // rdc 文件中的压缩数据按page存储，每个page前4个字节存储长度, 由于对齐原因此长度有可能大于 decodedSize
                            int dataSize = *(int*)pDiskData; pDiskData += 4;
                            Debug.Assert(dataSize > 0 && dataSize < outBuffLen);

                            int decodedSize = decoder.LZ4_decompress_safe_continue(pDiskData, pOutBuffer, dataSize, LZ4_BLOCK_SIZE);
                            Debug.Assert(decodedSize > 0 && decodedSize <= outBuffLen);
                            pDiskData += dataSize;

                            rawStream.Write(useBuff0 ? outBuffer0 : outBuffer1, 0, decodedSize);
                            loopNo++;
                        }
                    }
                }

                uncompressedData = rawStream.ToArray();
                Debug.Assert(uncompressedData.Length == (int)header.sectionUncompressedLength);
            }
            else if ((header.sectionFlags & SectionFlags.ZstdCompressed) == SectionFlags.ZstdCompressed)
            {
                using (var decoder = new Decompressor())
                {
                    uncompressedData = decoder.Unwrap(diskData);
                }
            }
            else // raw data
            {
                uncompressedData = diskData;
            }
        }

        public void CompressData(SectionFlags flags)
        {

        }

        /// <summary>
        /// 获取Chunks信息
        /// </summary>
        /// <returns></returns>
        public string DumpChunkInfos()
        {
            if (chunkMetas == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0, imax = chunkMetas.Count; i < imax; i++)
            {
                var chunk = chunkMetas[i];
                sb.AppendLine($"{i + 1,-4}  {chunk, -30}  {chunk.chunkID, -4}  offset:{chunk.offset,-8}  len:{chunk.fullLength,-8}".Trim());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将FontTexture数据保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveFontTexture(string fileName)
        {
            //var chunkInfo = fontTextureChunk.updateSubresourceChunkInfo;

            //Bitmap bitmap = new Bitmap((int)chunkInfo.pBox.right, (int)chunkInfo.pBox.bottom, PixelFormat.Format24bppRgb);
            //BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //fixed (void* p = &uncompressedData[(int)chunkInfo.ContentsOffset]) // 一般此贴图都是2的幂，因此忽略stride判断
            //{
            //    byte* pSrc = (byte*)p;
            //    byte* pDst = (byte*)bitmapData.Scan0;

            //    int dataSize = (int)chunkInfo.ContentsLength;
            //    for (int i = 0; i < dataSize; i++)
            //    {
            //        byte val = *pSrc++;
            //        *pDst++ = val; // R
            //        *pDst++ = val; // G
            //        *pDst++ = val; // B
            //    }
            //}

            //bitmap.UnlockBits(bitmapData);

            //bitmap.Save(fileName);
        }

        /// <summary>
        /// 从图片加载FontTexture数据
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFontTexture(string fileName)
        {
            //var chunkInfo = fontTextureChunk.updateSubresourceChunkInfo;

            //Bitmap bitmap = new Bitmap(fileName, false);
            //if(bitmap.Width != (int)chunkInfo.pBox.right || bitmap.Height != (int)chunkInfo.pBox.bottom)
            //{
            //    Console.WriteLine($"字体贴图的尺寸必须为:{chunkInfo.pBox.right}*{chunkInfo.pBox.bottom}");
            //    return;
            //}

            //BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //fixed (void* p = &uncompressedData[(int)chunkInfo.ContentsOffset]) // 一般此贴图都是2的幂，因此忽略stride判断
            //{
            //    byte* pSrc = (byte*)bitmapData.Scan0;
            //    byte* pDst = (byte*)p;

            //    int dataSize = (int)chunkInfo.ContentsLength;
            //    for (int i = 0; i < dataSize; i++)
            //    {
            //        byte val = *pSrc;
            //        *pDst++ = val;

            //        pSrc += 3; // RGB
            //    }
            //}

            //bitmap.UnlockBits(bitmapData);
        }
    }

}
