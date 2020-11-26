using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    public unsafe class D3DTextureConvert
    {
        /// <summary>
        /// 将贴图chunk数据保存到文件，扩展名会自动设置
        /// </summary>
        /// <param name="texChunk"></param>
        /// <param name="path"></param>
        public static void SaveTextureToFile(Chunk_CreateTexture2D texChunk, string path)
        {
            D3D11_SUBRESOURCE_DATA[] subDatas = texChunk.pInitialDatas;
            if(subDatas == null)
            {
                Chunk_InitialContents initialChunk = texChunk.chunkManager.GetInitialContentsChunk(texChunk.resourceId);
                if (initialChunk != null)
                {
                    subDatas = initialChunk.subDatas;
                }
            }

            if(subDatas == null)
            {
                Console.WriteLine($"can not find Texture Data of {texChunk}");
                return;
            }

            SaveTextureToFile(subDatas, texChunk.Descriptor, texChunk.chunkManager.section, path);
        }

        /// <summary>
        /// 将swapbuffer chunk数据保存到文件
        /// </summary>
        /// <param name="swapChunk"></param>
        /// <param name="path"></param>
        public static void SaveTextureToFile(Chunk_CreateSwapBuffer swapChunk, string path)
        {
            D3D11_SUBRESOURCE_DATA[] subDatas = null;
            Chunk_InitialContents initialChunk = swapChunk.chunkManager.GetInitialContentsChunk(swapChunk.resourceId);
            if (initialChunk != null)
            {
                subDatas = initialChunk.subDatas;
            }

            if (subDatas == null)
                return;

            SaveTextureToFile(subDatas, swapChunk.BackbufferDescriptor, swapChunk.chunkManager.section, path);
        }

        /// <summary>
        /// 从图像文件加载贴图数据到Chunk, 要求图像尺寸与Chunk定义的完全一致
        /// </summary>
        /// <param name="texChunk"></param>
        /// <param name="path"></param>
        public static void LoadTextureDataFromFile(Chunk_CreateTexture2D texChunk, string path)
        {
            D3D11_SUBRESOURCE_DATA[] subDatas = texChunk.pInitialDatas;
            if (subDatas == null)
            {
                Chunk_InitialContents initialChunk = texChunk.chunkManager.GetInitialContentsChunk(texChunk.resourceId);
                if (initialChunk != null)
                {
                    subDatas = initialChunk.subDatas;
                }
            }

            if (subDatas == null)
            {
                Console.WriteLine($"can not find Texture Data of {texChunk}");
                return;
            }

            LoadTextureDataFromFile(subDatas, texChunk.Descriptor, texChunk.chunkManager.section, path);
        }

        /// <summary>
        /// 从文件加载数据到swapchunk
        /// </summary>
        /// <param name="swapChunk"></param>
        /// <param name="path"></param>
        public static void LoadTextureDataFromFile(Chunk_CreateSwapBuffer swapChunk, string path)
        {
            D3D11_SUBRESOURCE_DATA[] subDatas = null;
            if (subDatas == null)
            {
                Chunk_InitialContents initialChunk = swapChunk.chunkManager.GetInitialContentsChunk(swapChunk.resourceId);
                if (initialChunk != null)
                {
                    subDatas = initialChunk.subDatas;
                }
            }

            if (subDatas == null)
            {
                Console.WriteLine($"can not find Texture Data of {swapChunk}");
                return;
            }

            LoadTextureDataFromFile(subDatas, swapChunk.BackbufferDescriptor, swapChunk.chunkManager.section, path);
        }

        /// <summary>
        /// 保存贴图到文件，适用于 Chunk_CreateTexture2D 及 Chunk_CreateSwapBuffer
        /// </summary>
        /// <param name="subDatas"></param>
        /// <param name="desc"></param>
        /// <param name="section"></param>
        public static void SaveTextureToFile(D3D11_SUBRESOURCE_DATA[] subDatas, D3D11_TEXTURE2D_DESC desc, Section section, string path)
        {
            DXGI_FORMAT format = desc.Format;
            if (Common.IsBlockFormat(format)) // 暂时不支持导出压缩格式
                return;

            if (format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM
                || format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS
                || format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB) // 4通道保存成tga以方便编辑A通道
            {
                FIBITMAP bmp = FreeImage.AllocateT(FREE_IMAGE_TYPE.FIT_BITMAP, (int)desc.Width, (int)desc.Height, 32);

                Debug.Assert(subDatas[0].sysMemLength >= desc.Width * desc.Height * 4); // sysMemLength 是对齐的，因此可能大于真实数据大小

                fixed (void* p = &section.uncompressedData[subDatas[0].sysMemDataOffset])
                {
                    byte* rawData = (byte*)p;
                    for (int h = (int)(desc.Height - 1); h >= 0; h--) // bitmap 存储方向与 dx 相反
                    {
                        byte* scanLine = (byte*)FreeImage.GetScanLine(bmp, h);
                        for (int w = 0; w < desc.Width; w++)
                        {
                            *scanLine++ = rawData[w * 4];
                            *scanLine++ = rawData[w * 4 + 1];
                            *scanLine++ = rawData[w * 4 + 2];
                            *scanLine++ = rawData[w * 4 + 3];
                        }

                        rawData += subDatas[0].SysMemPitch;
                    }
                }

                path = $"{path}.tga";
                File.Delete(path);
                FreeImage.Save(FREE_IMAGE_FORMAT.FIF_TARGA, bmp, path, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
                FreeImage.Unload(bmp);
            }
            else if (format == DXGI_FORMAT.DXGI_FORMAT_R8_SNORM
                || format == DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS
                || format == DXGI_FORMAT.DXGI_FORMAT_R8_UNORM
                || format == DXGI_FORMAT.DXGI_FORMAT_A8_UNORM) // 经过测试单通道无论指定哪个通道数据都只存了一个通道, 单通道保存成png24
            {
                FIBITMAP bmp = FreeImage.AllocateT(FREE_IMAGE_TYPE.FIT_BITMAP, (int)desc.Width, (int)desc.Height, 24);
                Debug.Assert(subDatas[0].sysMemLength >= desc.Width * desc.Height);
                fixed (void* p = &section.uncompressedData[subDatas[0].sysMemDataOffset])
                {
                    byte* rawData = (byte*)p;
                    for (int h = (int)(desc.Height - 1); h >= 0; h--) // bitmap 存储方向与 dx 相反
                    {
                        byte* scanLine = (byte*)FreeImage.GetScanLine(bmp, h);
                        for (int w = 0; w < desc.Width; w++)
                        {
                            byte val = rawData[w];
                            *scanLine++ = val;
                            *scanLine++ = val;
                            *scanLine++ = val;
                        }

                        rawData += subDatas[0].SysMemPitch;
                    }
                }

                path = $"{path}.bmp";
                File.Delete(path);
                FreeImage.Save(FREE_IMAGE_FORMAT.FIF_BMP, bmp, path, FREE_IMAGE_SAVE_FLAGS.BMP_SAVE_RLE);
                FreeImage.Unload(bmp);
            }
        }

        /// <summary>
        /// 从文件加载贴图数据
        /// </summary>
        /// <param name="subDatas"></param>
        /// <param name="desc"></param>
        /// <param name="section"></param>
        /// <param name="path"></param>
        public static void LoadTextureDataFromFile(D3D11_SUBRESOURCE_DATA[] subDatas, D3D11_TEXTURE2D_DESC desc, Section section, string path)
        {
            FREE_IMAGE_FORMAT fiFormat = default;

            string ext = Path.GetExtension(path).ToLower();

            if (ext == ".bmp")
                fiFormat = FREE_IMAGE_FORMAT.FIF_BMP;
            else if (ext == ".tga")
                fiFormat = FREE_IMAGE_FORMAT.FIF_TARGA;
            else
            {
                Console.WriteLine($"unsupported format {ext}");
                return;
            }

            FIBITMAP bmp = FreeImage.Load(fiFormat, path, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
            if (bmp == null || bmp.IsNull)
            {
                Console.WriteLine($"加载图片文件失败 {path}");
                return;
            }

            try
            {
                DXGI_FORMAT format = desc.Format;
                if (Common.IsBlockFormat(format)) // 暂时不支持压缩格式
                    return;

                uint fiBpp = FreeImage.GetBPP(bmp) / 8;
                uint width = FreeImage.GetWidth(bmp);
                uint height = FreeImage.GetHeight(bmp);

                if (width != desc.Width || height != desc.Height)
                {
                    Console.WriteLine($"size unmatch of file {path}, should be {desc.Width}*{desc.Height}");
                    return;
                }

                if (format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM
                    || format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS
                    || format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB) // 4通道保存成tga以方便编辑A通道
                {
                    Debug.Assert(subDatas[0].sysMemLength >= desc.Width * desc.Height);

                    fixed (void* p = &section.uncompressedData[subDatas[0].sysMemDataOffset])
                    {
                        byte* rawData = (byte*)p;
                        for (int h = (int)(desc.Height - 1); h >= 0; h--) // bitmap 存储方向与 dx 相反
                        {
                            byte* scanLine = (byte*)FreeImage.GetScanLine(bmp, h);
                            for (int w = 0; w < desc.Width; w++)
                            {
                                rawData[w * 4] = *scanLine++;
                                rawData[w * 4 + 1] = *scanLine++;
                                rawData[w * 4 + 2] = *scanLine++;
                                rawData[w * 4 + 3] = *scanLine++;
                            }

                            rawData += subDatas[0].SysMemPitch;
                        }
                    }
                }
                else if (format == DXGI_FORMAT.DXGI_FORMAT_R8_SNORM
                    || format == DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS
                    || format == DXGI_FORMAT.DXGI_FORMAT_R8_UNORM
                    || format == DXGI_FORMAT.DXGI_FORMAT_A8_UNORM) // 经过测试单通道无论指定哪个通道数据都只存了一个通道, 单通道保存成png24
                {
                    Debug.Assert(subDatas[0].sysMemLength >= desc.Width * desc.Height);

                    fixed (void* p = &section.uncompressedData[subDatas[0].sysMemDataOffset])
                    {
                        byte* rawData = (byte*)p;
                        for (int h = (int)(desc.Height - 1); h >= 0; h--) // bitmap 存储方向与 dx 相反
                        {
                            byte* scanLine = (byte*)FreeImage.GetScanLine(bmp, h);
                            for (int w = 0; w < desc.Width; w++)
                            {
                                rawData[w] = *scanLine;
                                scanLine += fiBpp;
                            }

                            rawData += subDatas[0].SysMemPitch;
                        }
                    }
                }
            }
            finally
            {
                FreeImage.Unload(bmp);
            }
        }
    }
}
