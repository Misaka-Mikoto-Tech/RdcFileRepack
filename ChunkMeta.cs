using LitJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    public enum SDChunkFlags : ulong
    {
        NoFlags = 0x0,
        OpaqueChunk = 0x1,
        HasCallstack = 0x2,
    }

    /// <summary>
    /// Rdc文件中的Chunk原始数据
    /// </summary>
    public unsafe class ChunkMeta : ISerializable
    {
        public int index { get; private set; }
        public int eventId { get; private set; }

        public bool isRemoved;

        public long offset;
        public long headerLength;
        public long fullLength { get { return headerLength + (long)length; } }

        public ChunkFlags flags;

        public uint chunkID;
        public SDChunkFlags chunkFlags;
        public ulong length; // data length
        public ulong threadID;
        public long durationMicro = -1;
        public ulong timestampMicro;
        public ulong[] callstack;

        public D3D11Chunk chunkType;

        public ChunkMeta(int index, int eventId)
        {
            this.index = index;
            this.eventId = eventId;
            isRemoved = false;
        }

        public override string ToString()
        {
            if (chunkType < D3D11Chunk.DeviceInitialisation)
                return ((SystemChunk)chunkType).ToString();
            else
                return chunkType.ToString();
        }

        public void LoadFromJson(JsonData jsonData, StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public int LoadFromStream(BinaryReader br)
        {
            var ms = br.BaseStream;
            ms.AlignUp(64);

            offset = ms.Position;

            BeginChunk(br);
            ProcessChunk(br);
            EndChunk(br);

            return (int)(ms.Position - offset);
        }

        protected void BeginChunk(BinaryReader br)
        {
            flags = (ChunkFlags)br.ReadUInt32();
            Debug.Assert(flags != 0);

            chunkID = (uint)(flags & ChunkFlags.ChunkIndexMask);
            chunkType = (D3D11Chunk)chunkID;
            chunkFlags = SDChunkFlags.NoFlags;

            if ((flags & ChunkFlags.ChunkCallstack) != 0)
            {
                uint numFrames = br.ReadUInt32();
                if (numFrames < 4096)
                {
                    chunkFlags |= SDChunkFlags.HasCallstack;
                    callstack = new ulong[numFrames];
                    br.Read(callstack, 0, callstack.Length);
                }
                else
                {
                    Console.WriteLine($"Read invalid number of callstack frames:{numFrames}");
                    br.BaseStream.Position += numFrames * sizeof(ulong);
                }
            }

            if ((flags & ChunkFlags.ChunkThreadID) != 0)
                threadID = br.ReadUInt64();
            else
                threadID = 0;

            if ((flags & ChunkFlags.ChunkDuration) != 0)
                durationMicro = br.ReadInt64();
            else
                durationMicro = -1;

            if ((flags & ChunkFlags.ChunkTimestamp) != 0)
                timestampMicro = br.ReadUInt64();
            else
                timestampMicro = 0;

            if ((flags & ChunkFlags.Chunk64BitSize) != 0)
                length = br.ReadUInt64();
            else
                length = br.ReadUInt32();

            headerLength = br.BaseStream.Position - offset;
        }

        protected void ProcessChunk(BinaryReader br)
        {
            
        }

        protected void EndChunk(BinaryReader br)
        {
            var ms = br.BaseStream;
            ms.Position = offset + fullLength;
            ms.AlignUp(64);
        }

        public void SaveToJson(StringBuilder sb, JsonData jsonData)
        {
            throw new NotImplementedException();
        }

        public int SaveToStream(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }

        public class UpdateSubresourceChunkInfo
        {
            public enum D3D11_COPY_FLAGS : uint
            {
                D3D11_COPY_NO_OVERWRITE = 0x1,
                D3D11_COPY_DISCARD = 0x2
            }

            public struct D3D11_BOX
            {
                public bool isExist;
                public uint left;
                public uint top;
                public uint front;
                public uint right;
                public uint bottom;
                public uint back;
            }

            public ulong m_CurContextId;
            public ulong pDstResource;
            public uint DstSubresource;
            public D3D11_BOX pBox;
            public uint SrcRowPitch;
            public uint SrcDepthPitch;
            public D3D11_COPY_FLAGS CopyFlags;
            public bool IsUpdate;

            public ulong ContentsLength;
            //public byte[] Contents;

            public long ContentsOffset; // 数据在流中的偏移量


            public void LoadFromStream(BinaryReader br)
            {
                m_CurContextId = br.ReadUInt64();
                pDstResource = br.ReadUInt64();
                DstSubresource = br.ReadUInt32();
                {// pBox
                    pBox.isExist = br.ReadBoolean();
                    pBox.left = br.ReadUInt32();
                    pBox.top = br.ReadUInt32();
                    pBox.front = br.ReadUInt32();
                    pBox.right = br.ReadUInt32();
                    pBox.bottom = br.ReadUInt32();
                    pBox.back = br.ReadUInt32();
                }
                SrcRowPitch = br.ReadUInt32();
                SrcDepthPitch = br.ReadUInt32();
                CopyFlags = (D3D11_COPY_FLAGS)br.ReadUInt32();
                IsUpdate = br.ReadBoolean();
                ContentsLength = br.ReadUInt64();
                br.BaseStream.AlignUp(64);
                ContentsOffset = br.BaseStream.Position;

                // check
                long newOffset = ContentsOffset + (long)ContentsLength;
                br.BaseStream.Position = newOffset;
                ulong newContentLength = br.ReadUInt64();
                Debug.Assert(ContentsLength == newContentLength); // renderdoc 在buffer后面还存了一个长度，用来校验读取是否正确
            }
        }
    }
}
