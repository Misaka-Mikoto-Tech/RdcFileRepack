﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    public interface IChunk
    {
        int index { get; }
        int eventId { get; }
        bool isRemoved { get; }
        /// <summary>
        /// 资源名，由Chunk_SetDebugName设置, 某些chunk可能没有name
        /// </summary>
        string name { get; set; }
        /// <summary>
        /// 资源id，非资源chunk此字段为默认值0
        /// </summary>
        ulong resourceId { get; }
        /// <summary>
        /// 父id，此时当前chunk一般用来给parentId设置参数或填充数据
        /// </summary>
        ulong parentId { get; }
        ChunkMeta chunkMeta { get; }
        /// <summary>
        /// 子节点，包括设置名字，填充数据等引用到当前chunk的其它chunk
        /// </summary>
        List<IChunk> children { get; }
        IChunk parent { get; set; }

        void Load(ChunkMeta meta, BinaryReader br);
        /// <summary>
        /// 加载完成后的二次处理（可以通过ID访问到其它的Chunk）
        /// </summary>
        void PostLoaded();
    }

    public class ChunkBase : IChunk
    {
        public int index => chunkMeta.index;
        public int eventId => chunkMeta.eventId;
        public bool isRemoved => chunkMeta.isRemoved;
        /// <summary>
        /// 资源名，由Chunk_SetDebugName设置, 某些chunk可能没有name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 资源id，非资源chunk此字段为默认值0
        /// </summary>
        public ulong resourceId { get; protected set; }
        /// <summary>
        /// 父id，此时当前chunk一般用来给parentId设置参数或填充数据
        /// </summary>
        public ulong parentId { get; protected set; }

        public ChunkMeta chunkMeta { get; protected set; }

        public List<IChunk> children { get; protected set; } = new List<IChunk>();

        public IChunk parent { get; set; }

        public ChunkManager chunkManager { get; protected set; }

        public ChunkBase(ChunkManager chunkManager)
        {
            this.chunkManager = chunkManager;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
                return chunkMeta.ToString();
            else
                return $"{chunkMeta, -40}{name} eid:{chunkMeta.eventId}";
        }

        public virtual void Load(ChunkMeta meta, BinaryReader br)
        {
            chunkMeta = meta;
            var ms = br.BaseStream;
            ms.Position = meta.offset + meta.headerLength;
        }

        public virtual void PostLoaded()
        {
        }
    }

    /// <summary>
    /// 所有eventId大于0的都需要继承自此类
    /// </summary>
    public class Chunk_Event : ChunkBase
    {
        public ulong m_CurContextId;

        public Chunk_Event(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            m_CurContextId = br.ReadUInt64();
        }
    }

    public class Chunk_DriverInit : ChunkBase
    {
        public D3D11InitParams initParams;

        public Chunk_DriverInit(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            initParams = D3D11Reader.Read_D3D11InitParams(br) as D3D11InitParams;
        }

        /// <summary>
        /// 修改设备名
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="name"></param>
        public void ModifyDeviceName(BinaryWriter bw, string name)
        {
            int maxLen = initParams.AdapterDesc.DescriptionLen;
            if(name.Length > maxLen)
            {
                Console.WriteLine($"最大设备名称长度为 {maxLen}");
                return;
            }

            bw.BaseStream.Position = initParams.AdapterDesc.offset; // Description 为第一个字段
            Utils.WriteChunkString(bw, name, maxLen);
            initParams.AdapterDesc.Description = name;
        }
    }

    public class Chunk_CreateTexture2D : ChunkBase
    {
        public D3D11_TEXTURE2D_DESC Descriptor;
        /// <summary>
        /// 创建时附带的初始化数据，此处可能为null，数据可能会被其它的 Chunk_InitialContents 初始化
        /// </summary>
        public D3D11_SUBRESOURCE_DATA[] pInitialDatas;

        public Chunk_CreateTexture2D(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);
            Descriptor = D3D11Reader.Read_D3D11_TEXTURE2D_DESC(br) as D3D11_TEXTURE2D_DESC;

            // unused, just for the sake of the user
            {
                uint numSubresources = Descriptor.MipLevels != 0
                                           ? Descriptor.MipLevels
                                           : Common.CalcNumMips(Descriptor.Width, Descriptor.Height, 1);
                numSubresources *= Descriptor.ArraySize;

                pInitialDatas = D3D11Reader.Read_D3D11_Array<D3D11_SUBRESOURCE_DATA>(br); // 初始化第一步
                resourceId = br.ReadUInt64();

                if(pInitialDatas != null)
                {
                    D3D11Reader.Read_CreateTextureData(br, pInitialDatas, Descriptor.Width, Descriptor.Height, 1, Descriptor.Format,
                    Descriptor.MipLevels, Descriptor.ArraySize, pInitialDatas != null); // 初始化第二步
                }
            }
        }
    }

    /// <summary>
    /// 目前好像没见用的
    /// </summary>
    public class Chunk_CreateTexture2D1 : ChunkBase
    {
        /// <summary>
        /// 有时数据是通过其它chunk初始化的
        /// </summary>
        public List<IChunk> resourceChunk = new List<IChunk>();
        /// <summary>
        /// 此Texture关联的View列表
        /// </summary>
        public List<IChunk> views = new List<IChunk>();
        /// <summary>
        /// 相关联的设置名称Chunk
        /// </summary>
        public IChunk setResourceNameChunk;

        public Chunk_CreateTexture2D1(ChunkManager chunkManager) : base(chunkManager) { }
    }

    public class Chunk_SetResourceName : ChunkBase
    {
        public Chunk_SetResourceName(ChunkManager chunkManager) : base(chunkManager) { }
        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            parentId = br.ReadUInt64(); // pResource
            name = Utils.ReadChunkString(br); // Name
        }

        public override void PostLoaded()
        {
            if(parent == null)
            {
                // 有一些资源类型（eg. shader, blendstate）并没有解析，所以会找不到对应Chunk
                return;
            }

            parent.name = name;
        }
    }

    public class Chunk_CreateSwapBuffer : ChunkBase
    {
        /// <summary>
        /// 有时数据是通过其它chunk初始化的(InitialContents, UpdateSubresource)
        /// </summary>
        public List<IChunk> resourceChunk = new List<IChunk>();
        public D3D11_TEXTURE2D_DESC BackbufferDescriptor;

        public Chunk_CreateSwapBuffer(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            uint Buffer = br.ReadUInt32();
            resourceId = br.ReadUInt64(); // SwapbufferID
            BackbufferDescriptor = D3D11Reader.Read_D3D11_TEXTURE2D_DESC(br) as D3D11_TEXTURE2D_DESC;

            name = "Serialised Swap Chain Buffer"; // fakeBB
        }
    }

    public class Chunk_CreateRenderTargetView : ChunkBase
    {
        public D3D11_RENDER_TARGET_VIEW_DESC pDesc;

        public Chunk_CreateRenderTargetView(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            parentId = br.ReadUInt64(); // pResource
            pDesc = D3D11Reader.Read_D3D11_Nullable<D3D11_RENDER_TARGET_VIEW_DESC>(br);
            resourceId = br.ReadUInt64(); // pView
        }
    }

    public class Chunk_CreateShaderResourceView : ChunkBase
    {
        public D3D11_SHADER_RESOURCE_VIEW_DESC pDesc;

        public Chunk_CreateShaderResourceView(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);
            
            parentId = br.ReadUInt64(); // pResource
            pDesc = D3D11Reader.Read_D3D11_Nullable<D3D11_SHADER_RESOURCE_VIEW_DESC>(br);
            resourceId = br.ReadUInt64(); // pView;
        }
    }

    public class Chunk_CreateDepthStencilView : ChunkBase
    {
        public D3D11_DEPTH_STENCIL_VIEW_DESC pDesc;

        public Chunk_CreateDepthStencilView(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            parentId = br.ReadUInt64(); // pResource
            pDesc = D3D11Reader.Read_D3D11_Nullable<D3D11_DEPTH_STENCIL_VIEW_DESC>(br);
            resourceId = br.ReadUInt64(); // pView
        }
    }

    public class Chunk_CreateBuffer : ChunkBase
    {
        public D3D11_BUFFER_DESC Descriptor;
        public D3D11_SUBRESOURCE_DATA pInitialData;
        public D3D11_SUBRESOURCE_DATA data;

        public Chunk_CreateBuffer(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            Descriptor = D3D11Reader.Read_D3D11_BUFFER_DESC(br) as D3D11_BUFFER_DESC; // Descriptor
            pInitialData = D3D11Reader.Read_D3D11_Nullable<D3D11_SUBRESOURCE_DATA>(br); // pInitialData
            resourceId = br.ReadUInt64(); // pBuffer

            int dataOffset;
            int count;
            D3D11Reader.Read_BytesArray(br, out dataOffset, out count, true);
            ulong InitialDataLength = br.ReadUInt64();
            Debug.Assert((int)InitialDataLength == count);

            data = new D3D11_SUBRESOURCE_DATA();
            data.pSysMem = null;
            data.SysMemPitch = Descriptor.ByteWidth;
            data.SysMemSlicePitch = Descriptor.ByteWidth;
            data.sysMemDataOffset = dataOffset;
            data.sysMemLength = count;
        }
    }

    public class Chunk_InitialContents : ChunkBase
    {
        public D3D11ResourceType type;
        public D3D11_SUBRESOURCE_DATA[] subDatas;

        public Chunk_InitialContents(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            type = (D3D11ResourceType)br.ReadUInt32();
            parentId = br.ReadUInt64(); // id

            // RenderDoc 可以保证对应的资源已创建
            D3D11_TEXTURE2D_DESC desc;

            var createChunk = chunkManager.GetResourceChunk(parentId);
            if (createChunk == null)
            {
                Console.WriteLine($"unsupported resouce on [Chunk_InitialContents] with id {parentId}");
                return; // 尚未支持的资源类型
            }
            else
            {
                if (createChunk is Chunk_CreateTexture2D)
                    desc = (createChunk as Chunk_CreateTexture2D).Descriptor;
                else if (createChunk is Chunk_CreateSwapBuffer)
                    desc = (createChunk as Chunk_CreateSwapBuffer).BackbufferDescriptor;
                else
                    throw new Exception($"unknown resource type {createChunk.GetType().Name}");
            }

            if (type == D3D11ResourceType.Resource_UnorderedAccessView)
            {
                // TODO
            }
            else if(type == D3D11ResourceType.Resource_Buffer)
            {
                // TODO
            }
            else if(type == D3D11ResourceType.Resource_Texture1D)
            {
                // TODO
            }
            else if(type == D3D11ResourceType.Resource_Texture2D)
            {
                uint NumSubresources = desc.MipLevels * desc.ArraySize;
                bool multisampled = desc.SampleDesc.Count > 1 || desc.SampleDesc.Quality > 0;

                if (multisampled)
                    NumSubresources *= desc.SampleDesc.Count;

                uint numReaded = br.ReadUInt32();
                Debug.Assert(NumSubresources == numReaded);
                NumSubresources = numReaded;

                bool OmittedContents = br.ReadBoolean(); // for compatibility

                if (OmittedContents)
                    return;

                subDatas = new D3D11_SUBRESOURCE_DATA[NumSubresources];

                for(int i = 0; i < NumSubresources; i++)
                {
                    // 数据格式与 CreteTexture2D 时的SubResource不同，原因不明, 可能是历史遗留问题？
                    uint numRows = Math.Max(1, desc.Height >> i);
                    if (Common.IsBlockFormat(desc.Format))
                        numRows = Common.AlignUp4(numRows) / 4;
                    else if (Common.IsYUVPlanarFormat(desc.Format))
                        numRows = Common.GetYUVNumRows(desc.Format, desc.Height);

                    subDatas[i] = new D3D11_SUBRESOURCE_DATA();
                    subDatas[i].SysMemPitch = br.ReadUInt32();
                    subDatas[i].SysMemSlicePitch = subDatas[i].SysMemPitch * numRows;
                    subDatas[i].pSysMem = D3D11Reader.Read_BytesArray(br, out subDatas[i].sysMemDataOffset, out subDatas[i].sysMemLength, true);
                }
            }
        }

        public override void PostLoaded()
        {
            if (parent == null)
                return;

            Debug.Assert(!chunkManager.initialContentChunks.ContainsKey(parentId));

            chunkManager.initialContentChunks[parentId] = this;
        }

        public override string ToString()
        {
            if (parent == null || string.IsNullOrEmpty(parent.name))
                return base.ToString();
            else
                return $"{chunkMeta,-40}{parent.name}";
        }
    }

    public class Chunk_UpdateSubresource : ChunkBase
    {
        public Chunk_UpdateSubresource(ChunkManager chunkManager) : base(chunkManager) { }
    }

    public class Chunk_UpdateSubresource1 : ChunkBase
    {
        public Chunk_UpdateSubresource1(ChunkManager chunkManager) : base(chunkManager) { }
    }

    #region EventChunk
    public class Chunk_IASetVertexBuffers : Chunk_Event
    {
        public uint StartSlot;
        public uint NumBuffers;
        public ulong[] ppVertexBuffers; // parentIds
        public uint[] pStrides;
        public uint[] pOffsets;


        public Chunk_IASetVertexBuffers(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            StartSlot = br.ReadUInt32();
            NumBuffers = br.ReadUInt32();
            ppVertexBuffers = D3D11Reader.Read_Primitive_Array<ulong>(br);
            pStrides = D3D11Reader.Read_Primitive_Array<uint>(br);
            pOffsets = D3D11Reader.Read_Primitive_Array<uint>(br);

            if(NumBuffers > 0)
            {
                parentId = ppVertexBuffers[0]; // 没办法，只显示第一个吧
            }
        }
    }

    public class Chunk_IASetIndexBuffer : Chunk_Event
    {
        public DXGI_FORMAT Format;
        public uint Offset;

        public Chunk_IASetIndexBuffer(ChunkManager chunkManager) : base(chunkManager) { }

        public override void Load(ChunkMeta meta, BinaryReader br)
        {
            base.Load(meta, br);

            parentId = br.ReadUInt64(); // pIndexBuffer
            Format = (DXGI_FORMAT)br.ReadInt32();
            Offset = br.ReadUInt32();
        }
    }
    #endregion
}
