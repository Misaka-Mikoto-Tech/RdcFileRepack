using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    /// <summary>
    /// UID Drawcall 使用的buffer数据修改器
    /// </summary>
    public static unsafe class UIDBufferModifier
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VertexBufferFormat
        {
            public float x;
            public float y;
            public float z;

            public byte r;
            public byte g;
            public byte b;
            public byte a;

            public float u;
            public float v;
        }

        /// <summary>
        /// 修改Buffer
        /// </summary>
        /// <param name="chunkManager"></param>
        /// <param name="eventId"></param>
        /// <param name="fillWithNumIndex"></param>
        /// <param name="characterWidth">字符宽度</param>
        public static void Modify(ChunkManager chunkManager, int eventId, int fillWithNumIndex, float characterWidth = 8f)
        {

            Chunk_IASetVertexBuffers chunk = chunkManager.allChunks[eventId + chunkManager.CaptureBeginChunkIndex - 1] as Chunk_IASetVertexBuffers;
            if (chunk == null)
            {
                Console.WriteLine($"chunk {eventId} is not a valid Chunk_IASetVertexBuffers");
                return;
            }

            uint stride = chunk.pStrides[0];
            uint offset = chunk.pOffsets[0];

            Chunk_CreateBuffer createBuffer = chunk.parent as Chunk_CreateBuffer;
            if (createBuffer == null)
            {
                Console.WriteLine("chunk dos not has a valid CreateBuffer parent");
                return;
            }

            int dataOffset = createBuffer.pInitialData != null ? createBuffer.pInitialData.sysMemDataOffset : createBuffer.data.sysMemDataOffset;

            const int UID_PREFIX_COUNT = 5; // 前5个文字为 <UID: >前缀

            uint perDrawLen = createBuffer.Descriptor.ByteWidth / 5; // 带阴影的文字会画5遍
            uint uidCount = (perDrawLen / stride) / 6 - UID_PREFIX_COUNT; // uid采用每个文字使用6个顶点的方式
            uint uidDataOffset = UID_PREFIX_COUNT * 6 * stride + offset;

            // 找出指定字符使用的6个顶点的uv
            VertexBufferFormat[] fillVal = new VertexBufferFormat[6];
            fixed(void * pFill = &chunkManager.section.uncompressedData[dataOffset + uidDataOffset + fillWithNumIndex * 6 * stride])
            {
                VertexBufferFormat* pUidFill = (VertexBufferFormat*)pFill;

                for(int i = 0; i < 6; i++)
                {
                    fillVal[i].u = pUidFill->u;
                    fillVal[i].v = pUidFill->v;

                    pUidFill++;
                }
                
            }

            // 开始替换数据
            int perDrawUidDataOffset = (int)uidDataOffset;
            for (int i = 0; i < 5; i++) // 带阴影的文字会画5遍
            {
                fixed(void * pData = &chunkManager.section.uncompressedData[dataOffset + perDrawUidDataOffset])
                {
                    VertexBufferFormat* pDst = (VertexBufferFormat*)pData;

                    for (int j = 0; j < uidCount * 6; j++)
                    {
                        // 当前轮次都以第一个字符的位置为基准
                        if(j < 6)
                        {
                            fillVal[j].x = pDst->x;
                            fillVal[j].y = pDst->y;
                            fillVal[j].z = pDst->z;
                        }

                        pDst->x = fillVal[j % 6].x + (j / 6) * characterWidth;
                        pDst->y = fillVal[j % 6].y;
                        pDst->z = fillVal[j % 6].z;

                        pDst->u = fillVal[j % 6].u;
                        pDst->v = fillVal[j % 6].v;

                        pDst++;
                    }
                }

                perDrawUidDataOffset += (int)perDrawLen;
            }
        }
    }
}
