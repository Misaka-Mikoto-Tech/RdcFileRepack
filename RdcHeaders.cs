using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using System.Diagnostics;
using System.Threading;
using ZstdNet;

namespace Rdc
{
    public interface ISerializable
    {
        int LoadFromStream(BinaryReader br);
        int SaveToStream(BinaryWriter bw);
    }

    public unsafe class FileHeader : ISerializable
    {
        public static readonly byte[] MAGIC_HEADER = { (byte)'R', (byte)'D', (byte)'O', (byte)'C', 0 };
        public const int MAGIC_LENGTH = 8;
        public const int VERSION_LENGTH = 16;

        public byte[] magic; // 8
        public uint version;
        public uint headerLength;
        public byte[] progVersion; // 16

        public bool IsValid()
        {
            if (magic == null || magic.Length < 8)
                return false;

            for(int i = 0, imax = MAGIC_HEADER.Length; i < imax; i++)
            {
                if (magic[i] != MAGIC_HEADER[i])
                    return false;
            }
            return true;
        }

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            magic = new byte[MAGIC_LENGTH];
            br.Read(magic, 0, MAGIC_LENGTH);
            version = br.ReadUInt32();
            headerLength = br.ReadUInt32();
            progVersion = new byte[VERSION_LENGTH];
            br.Read(progVersion, 0, VERSION_LENGTH);

            return (int)(br.BaseStream.Position - offset);
        }

        public int SaveToStream(BinaryWriter bw)
        {
            long offset = bw.BaseStream.Position;

            bw.Write(magic);
            bw.Write(version);
            bw.Write(headerLength);
            bw.Write(progVersion);

            return (int)(bw.BaseStream.Position - offset);
        }
    }

    /// <summary>
    /// 二进制缩略图，这是个jpg文件
    /// </summary>
    public class BinaryThumbnail : ISerializable
    {
        public const int MAX_THUMBNAIL_SIZE = 10 * 1024 * 1024;

        public ushort width;
        public ushort height;
        //public uint length;
        public byte[] data;

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            width = br.ReadUInt16();
            height = br.ReadUInt16();
            uint dataLen = br.ReadUInt32();
            if (dataLen > MAX_THUMBNAIL_SIZE)
                throw new Exception($"Thumbnail byte length invalid: {dataLen}");

            data = new byte[dataLen];
            br.Read(data, 0, data.Length);

            return (int)(br.BaseStream.Position - offset);
        }

        public int SaveToStream(BinaryWriter bw)
        {
            long offset = bw.BaseStream.Position;

            bw.Write(width);
            bw.Write(height);
            bw.Write((uint)data.Length);
            bw.Write(data, 0, data.Length);

            return (int)(bw.BaseStream.Position - offset);
        }
    }

    public class CaptureMetaData : ISerializable
    {
        public ulong machineIdent;
        public RDCDriver driverID;
        //public byte driverNameLength;
        public string driverName;

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            machineIdent = br.ReadUInt64();
            driverID = (RDCDriver)br.ReadUInt32();
            driverName = Utils.GetStringFromStream<byte>(br);

            return (int)(br.BaseStream.Position - offset);
        }

        public int SaveToStream(BinaryWriter bw)
        {
            long offset = bw.BaseStream.Position;

            bw.Write(machineIdent);
            bw.Write((uint)driverID);
            Utils.WriteStringToStream<byte>(driverName, bw);

            return (int)(bw.BaseStream.Position - offset);
        }
    }

    public class BinarySectionHeader : ISerializable
    {
        public byte isASCII;
        public byte[] zero; // 3
        public SectionType sectionType;
        public ulong sectionCompressedLength;
        public ulong sectionUncompressedLength;
        public ulong sectionVersion;
        public SectionFlags sectionFlags;
        //public uint sectionNameLength;
        public string name;

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            isASCII = br.ReadByte();

            if(isASCII == 1)
            {
                throw new Exception("ascii format is unsupported");
            }

            zero = new byte[3];
            br.Read(zero, 0, 3);
            sectionType =  (SectionType)br.ReadUInt32();
            sectionCompressedLength = br.ReadUInt64();
            sectionUncompressedLength = br.ReadUInt64();
            sectionVersion = br.ReadUInt64();
            sectionFlags = (SectionFlags)br.ReadUInt32();
            name = Utils.GetStringFromStream<uint>(br);

            //br.ReadByte(); // SkipBytes(1), renderdoc 读取 name 少读了一个字节，然后又 skip 了一个字节，因此等于直接读name

            return (int)(br.BaseStream.Position - offset);
        }

        public int SaveToStream(BinaryWriter bw)
        {
            long offset = bw.BaseStream.Position;

            bw.Write(isASCII);
            bw.Write(zero, 0, 3);
            bw.Write((uint)sectionType);
            bw.Write(sectionCompressedLength);
            bw.Write(sectionUncompressedLength);
            bw.Write(sectionVersion);
            bw.Write((uint)sectionFlags);
            Utils.WriteStringToStream<uint>(name, bw);

            return (int)(bw.BaseStream.Position - offset);
        }

        /// <summary>
        /// 设置为未压缩格式
        /// </summary>
        /// <param name="uncompressedSize"></param>
        public void SetToUncompressFormat()
        {
            sectionFlags &= ~(SectionFlags.LZ4Compressed | SectionFlags.ZstdCompressed);
            sectionCompressedLength = sectionUncompressedLength;
        }
    }

    public class SectionProperties : ISerializable
    {
        public string name;
        public SectionType type;
        public SectionFlags flags;
        public ulong version;
        public ulong uncompressedSize;
        public ulong compressedSize;

        public int LoadFromStream(BinaryReader br)
        {
            throw new NotImplementedException();
        }

        public int SaveToStream(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class SectionLocation : ISerializable
    {
        public ulong headerOffset;
        public ulong dataOffset;
        public ulong diskLength;

        public int LoadFromStream(BinaryReader br)
        {
            throw new NotImplementedException();
        }

        public int SaveToStream(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class RDCThumb : ISerializable
    {
        public byte[] pixels;
        public uint len;
        public ushort width;
        public ushort height;
        public FileType format;

        public int LoadFromStream(BinaryReader br)
        {
            throw new NotImplementedException();
        }

        public int SaveToStream(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class ExtThumbnailHeader : ISerializable
    {
        public const int headerSize = 2 + 2 + 4 + 4;

        public ushort width;
        public ushort height;
        public uint len;
        public FileType format;

        public int LoadFromStream(BinaryReader br)
        {
            long offset = br.BaseStream.Position;

            width = br.ReadUInt16();
            height = br.ReadUInt16();
            len = br.ReadUInt32();
            format = (FileType)br.ReadUInt32();

            return (int)(br.BaseStream.Position - offset);
        }

        public int SaveToStream(BinaryWriter bw)
        {
            long offset = bw.BaseStream.Position;

            bw.Write(width);
            bw.Write(height);
            bw.Write(len);
            bw.Write((uint)format);

            return (int)(bw.BaseStream.Position - offset);
        }
    }

    

    public enum RDCDriver : uint
    {
        Unknown = 0,
        D3D11 = 1,
        OpenGL = 2,
        Mantle = 3,
        D3D12 = 4,
        D3D10 = 5,
        D3D9 = 6,
        Image = 7,
        Vulkan = 8,
        OpenGLES = 9,
        D3D8 = 10,
        MaxBuiltin,
        Custom = 100000,
        Custom0 = Custom,
        Custom1,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
        Custom7,
        Custom8,
        Custom9,
    }

    public enum SystemChunk : uint
    {
        // 0 is reserved as a 'null' chunk that is only for debug
        DriverInit = 1,
        InitialContentsList,
        InitialContents,
        CaptureBegin,
        CaptureScope,
        CaptureEnd,

        FirstDriverChunk = 1000,
    }

    public enum SectionType : uint
    {
        Unknown = 0,
        First = Unknown,
        FrameCapture,
        ResolveDatabase,
        Bookmarks,
        Notes,
        ResourceRenames,
        AMDRGPProfile,
        ExtendedThumbnail,
        Count,
    }

    public enum SectionFlags : uint
    {
        NoFlags = 0x0,
        ASCIIStored = 0x1,
        LZ4Compressed = 0x2,
        ZstdCompressed = 0x4,
    }

    public enum FileType : uint
    {
        DDS,
        First = DDS,
        PNG,
        JPG,
        BMP,
        TGA,
        HDR,
        EXR,
        Raw,
        Count,
    }

    [Flags]
    public enum ChunkFlags
    {
        ChunkIndexMask = 0x0000ffff,
        ChunkCallstack = 0x00010000,
        ChunkThreadID = 0x00020000,
        ChunkDuration = 0x00040000,
        ChunkTimestamp = 0x00080000,
        Chunk64BitSize = 0x00100000,
    };

}
