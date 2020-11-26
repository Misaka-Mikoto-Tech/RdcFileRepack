using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    /// <summary>
    /// Chunk管理器，考虑改为非单例模式以便同时加载多个rdc到内存
    /// </summary>
    public class ChunkManager
    {
        public Section section { get; private set; }
        public List<IChunk> allChunks { get; private set; } = new List<IChunk>();
        /// <summary>
        /// 资源chunk，key: ResourceID
        /// </summary>
        public Dictionary<ulong, IChunk> resourceChunks { get; private set; } = new Dictionary<ulong, IChunk>();
        /// <summary>
        /// 指定资源id与其初始化Chunk
        /// </summary>
        public Dictionary<ulong, IChunk> initialContentChunks { get; private set; } = new Dictionary<ulong, IChunk>();
        public Chunk_DriverInit driverInitChunk { get; private set; }

        public void LoadChunksFromSection(Section section)
        {
            Debug.Assert(section.header.sectionType == SectionType.FrameCapture);

            this.section = section;

            allChunks = new List<IChunk>();
            resourceChunks = new Dictionary<ulong, IChunk>();

            using (MemoryStream ms = new MemoryStream(section.uncompressedData))
            using (BinaryReader br = new BinaryReader(ms))
            {
                foreach (var meta in section.chunkMetas)
                {
                    IChunk chunk = CreateChunkByMeta(meta);
                    chunk.Load(meta, br);
                    AddChunk(chunk);

                    if (chunk is Chunk_DriverInit)
                        driverInitChunk = chunk as Chunk_DriverInit;
                }

                foreach(var chunk in allChunks)
                {
                    chunk.PostLoaded();
                }
            }
        }

        /// <summary>
        /// 增加一个Chunk，资源相关chunk会帮助链接父子关系（rdc文件会保证按先后顺序存储）
        /// </summary>
        /// <param name="chunk"></param>
        private void AddChunk(IChunk chunk)
        {
            allChunks.Add(chunk);

            if (chunk.resourceId != 0)
            {
                resourceChunks.Add(chunk.resourceId, chunk);
            }

            if (chunk.parentId != 0)
            {
                if (resourceChunks.TryGetValue(chunk.parentId, out IChunk parentChunk))
                {
                    chunk.parent = parentChunk;
                    parentChunk.children.Add(chunk);
                }
            }
        }

        public void SetDeviceName(string name)
        {
            using (MemoryStream ms = new MemoryStream(section.uncompressedData))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                if(driverInitChunk != null)
                {
                    driverInitChunk.ModifyDeviceName(bw, name);
                }
            }
        }

        public IChunk GetResourceChunk(ulong resourceId)
        {
            IChunk chunk;
            if (resourceChunks.TryGetValue(resourceId, out chunk))
                return chunk;
            else
                return null;
        }

        /// <summary>
        /// 获取某个资源id对应的初始化chunk
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public Chunk_InitialContents GetInitialContentsChunk(ulong resourceId)
        {
            if (initialContentChunks.TryGetValue(resourceId, out IChunk chunk))
                return chunk as Chunk_InitialContents;
            else
                return null;
        }

        private IChunk CreateChunkByMeta(ChunkMeta chunkMeta)
        {
            SystemChunk systemChunk = (SystemChunk)chunkMeta.chunkID;
            D3D11Chunk d3D11Chunk = (D3D11Chunk)chunkMeta.chunkID;

            if (chunkMeta.chunkID < (uint)SystemChunk.FirstDriverChunk)
            {
                switch(systemChunk)
                {
                    case SystemChunk.DriverInit:
                        return new Chunk_DriverInit(this);
                    case SystemChunk.InitialContents:
                        return new Chunk_InitialContents(this);
                    default:
                        return new ChunkBase(this);
                }
            }
            else
            {
                switch (d3D11Chunk)
                {
                    case D3D11Chunk.CreateTexture2D:
                        return new Chunk_CreateTexture2D(this);
                    case D3D11Chunk.CreateTexture2D1:
                        return new Chunk_CreateTexture2D1(this);
                    case D3D11Chunk.SetResourceName:
                        return new Chunk_SetResourceName(this);
                    case D3D11Chunk.CreateSwapBuffer:
                        return new Chunk_CreateSwapBuffer(this);
                    case D3D11Chunk.CreateRenderTargetView:
                        return new Chunk_CreateRenderTargetView(this);
                    case D3D11Chunk.CreateShaderResourceView:
                        return new Chunk_CreateShaderResourceView(this);
                    case D3D11Chunk.CreateDepthStencilView:
                        return new Chunk_CreateDepthStencilView(this);
                    case D3D11Chunk.UpdateSubresource:
                        return new Chunk_UpdateSubresource(this);
                    case D3D11Chunk.UpdateSubresource1:
                        return new Chunk_UpdateSubresource1(this);
                    case D3D11Chunk.CreateBuffer:
                        return new Chunk_CreateBuffer(this);
                    default:
                        return new ChunkBase(this);
                }
            }
        }
    }
}
