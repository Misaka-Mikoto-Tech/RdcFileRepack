using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class D3D11
{
    public static uint D3D11_SDK_VERSION = 7;
}

public class D3D11_Content
{
    public long offset;
    public long length;
}

public enum D3D_DRIVER_TYPE
{
    D3D_DRIVER_TYPE_UNKNOWN = 0,
    D3D_DRIVER_TYPE_HARDWARE = (D3D_DRIVER_TYPE_UNKNOWN + 1),
    D3D_DRIVER_TYPE_REFERENCE = (D3D_DRIVER_TYPE_HARDWARE + 1),
    D3D_DRIVER_TYPE_NULL = (D3D_DRIVER_TYPE_REFERENCE + 1),
    D3D_DRIVER_TYPE_SOFTWARE = (D3D_DRIVER_TYPE_NULL + 1),
    D3D_DRIVER_TYPE_WARP = (D3D_DRIVER_TYPE_SOFTWARE + 1)
}

public enum D3D_FEATURE_LEVEL
{
    D3D_FEATURE_LEVEL_1_0_CORE = 0x1000,
    D3D_FEATURE_LEVEL_9_1 = 0x9100,
    D3D_FEATURE_LEVEL_9_2 = 0x9200,
    D3D_FEATURE_LEVEL_9_3 = 0x9300,
    D3D_FEATURE_LEVEL_10_0 = 0xa000,
    D3D_FEATURE_LEVEL_10_1 = 0xa100,
    D3D_FEATURE_LEVEL_11_0 = 0xb000,
    D3D_FEATURE_LEVEL_11_1 = 0xb100,
    D3D_FEATURE_LEVEL_12_0 = 0xc000,
    D3D_FEATURE_LEVEL_12_1 = 0xc100
}

public struct LUID
{
    public uint LowPart;
    public int HighPart;
}

public class DXGI_ADAPTER_DESC : D3D11_Content
{
    public int DescriptionLen; // 原始的长度

    public string Description; // WCHAR[128], 文件中实际存储的是4字节长度变长字符串
    public uint VendorId;
    public uint DeviceId;
    public uint SubSysId;
    public uint Revision;
    public ulong DedicatedVideoMemory;
    public ulong DedicatedSystemMemory;
    public ulong SharedSystemMemory;
    public LUID AdapterLuid;
}

public class D3D11InitParams : D3D11_Content
{
    public D3D_DRIVER_TYPE DriverType = D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_UNKNOWN;
    public uint Flags = 0;
    public uint SDKVersion = D3D11.D3D11_SDK_VERSION;
    public uint NumFeatureLevels = 0;
    public D3D_FEATURE_LEVEL[] FeatureLevels = new D3D_FEATURE_LEVEL[16];

    public DXGI_ADAPTER_DESC AdapterDesc;

    // check if a frame capture section version is supported
    public const ulong CurrentVersion = 0x11;
}

public class D3D11_TEXTURE2D_DESC : D3D11_Content
{
    public uint Width;
    public uint Height;
    public uint MipLevels;
    public uint ArraySize;
    public DXGI_FORMAT Format;
    public DXGI_SAMPLE_DESC SampleDesc;
    public D3D11_USAGE Usage;
    public uint BindFlags;
    public uint CPUAccessFlags;
    public uint MiscFlags;
}

public class D3D11_RENDER_TARGET_VIEW_DESC : D3D11_Content
{
    public DXGI_FORMAT Format;
    public D3D11_RTV_DIMENSION ViewDimension;

    // 以下N选1
    public D3D11_BUFFER_RTV Buffer;
    public D3D11_TEX1D_RTV Texture1D;
    public D3D11_TEX1D_ARRAY_RTV Texture1DArray;
    public D3D11_TEX2D_RTV Texture2D;
    public D3D11_TEX2D_ARRAY_RTV Texture2DArray;
    public D3D11_TEX2DMS_RTV Texture2DMS;
    public D3D11_TEX2DMS_ARRAY_RTV Texture2DMSArray;
    public D3D11_TEX3D_RTV Texture3D;
}

public class D3D11_DEPTH_STENCIL_VIEW_DESC : D3D11_Content
{
    public DXGI_FORMAT Format;
    public D3D11_DSV_DIMENSION ViewDimension;
    public D3D11_DSV_FLAG Flags;

    // 以下N选1
    public D3D11_TEX1D_DSV Texture1D;
    public D3D11_TEX1D_ARRAY_DSV Texture1DArray;
    public D3D11_TEX2D_DSV Texture2D;
    public D3D11_TEX2D_ARRAY_DSV Texture2DArray;
    public D3D11_TEX2DMS_DSV Texture2DMS;
    public D3D11_TEX2DMS_ARRAY_DSV Texture2DMSArray;
}

public enum DXGI_FORMAT : uint
{
    DXGI_FORMAT_UNKNOWN	                                = 0,
    DXGI_FORMAT_R32G32B32A32_TYPELESS                   = 1,
    DXGI_FORMAT_R32G32B32A32_FLOAT                      = 2,
    DXGI_FORMAT_R32G32B32A32_UINT                       = 3,
    DXGI_FORMAT_R32G32B32A32_SINT                       = 4,
    DXGI_FORMAT_R32G32B32_TYPELESS                      = 5,
    DXGI_FORMAT_R32G32B32_FLOAT                         = 6,
    DXGI_FORMAT_R32G32B32_UINT                          = 7,
    DXGI_FORMAT_R32G32B32_SINT                          = 8,
    DXGI_FORMAT_R16G16B16A16_TYPELESS                   = 9,
    DXGI_FORMAT_R16G16B16A16_FLOAT                      = 10,
    DXGI_FORMAT_R16G16B16A16_UNORM                      = 11,
    DXGI_FORMAT_R16G16B16A16_UINT                       = 12,
    DXGI_FORMAT_R16G16B16A16_SNORM                      = 13,
    DXGI_FORMAT_R16G16B16A16_SINT                       = 14,
    DXGI_FORMAT_R32G32_TYPELESS                         = 15,
    DXGI_FORMAT_R32G32_FLOAT                            = 16,
    DXGI_FORMAT_R32G32_UINT                             = 17,
    DXGI_FORMAT_R32G32_SINT                             = 18,
    DXGI_FORMAT_R32G8X24_TYPELESS                       = 19,
    DXGI_FORMAT_D32_FLOAT_S8X24_UINT                    = 20,
    DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS                = 21,
    DXGI_FORMAT_X32_TYPELESS_G8X24_UINT                 = 22,
    DXGI_FORMAT_R10G10B10A2_TYPELESS                    = 23,
    DXGI_FORMAT_R10G10B10A2_UNORM                       = 24,
    DXGI_FORMAT_R10G10B10A2_UINT                        = 25,
    DXGI_FORMAT_R11G11B10_FLOAT                         = 26,
    DXGI_FORMAT_R8G8B8A8_TYPELESS                       = 27,
    DXGI_FORMAT_R8G8B8A8_UNORM                          = 28,
    DXGI_FORMAT_R8G8B8A8_UNORM_SRGB                     = 29,
    DXGI_FORMAT_R8G8B8A8_UINT                           = 30,
    DXGI_FORMAT_R8G8B8A8_SNORM                          = 31,
    DXGI_FORMAT_R8G8B8A8_SINT                           = 32,
    DXGI_FORMAT_R16G16_TYPELESS                         = 33,
    DXGI_FORMAT_R16G16_FLOAT                            = 34,
    DXGI_FORMAT_R16G16_UNORM                            = 35,
    DXGI_FORMAT_R16G16_UINT                             = 36,
    DXGI_FORMAT_R16G16_SNORM                            = 37,
    DXGI_FORMAT_R16G16_SINT                             = 38,
    DXGI_FORMAT_R32_TYPELESS                            = 39,
    DXGI_FORMAT_D32_FLOAT                               = 40,
    DXGI_FORMAT_R32_FLOAT                               = 41,
    DXGI_FORMAT_R32_UINT                                = 42,
    DXGI_FORMAT_R32_SINT                                = 43,
    DXGI_FORMAT_R24G8_TYPELESS                          = 44,
    DXGI_FORMAT_D24_UNORM_S8_UINT                       = 45,
    DXGI_FORMAT_R24_UNORM_X8_TYPELESS                   = 46,
    DXGI_FORMAT_X24_TYPELESS_G8_UINT                    = 47,
    DXGI_FORMAT_R8G8_TYPELESS                           = 48,
    DXGI_FORMAT_R8G8_UNORM                              = 49,
    DXGI_FORMAT_R8G8_UINT                               = 50,
    DXGI_FORMAT_R8G8_SNORM                              = 51,
    DXGI_FORMAT_R8G8_SINT                               = 52,
    DXGI_FORMAT_R16_TYPELESS                            = 53,
    DXGI_FORMAT_R16_FLOAT                               = 54,
    DXGI_FORMAT_D16_UNORM                               = 55,
    DXGI_FORMAT_R16_UNORM                               = 56,
    DXGI_FORMAT_R16_UINT                                = 57,
    DXGI_FORMAT_R16_SNORM                               = 58,
    DXGI_FORMAT_R16_SINT                                = 59,
    DXGI_FORMAT_R8_TYPELESS                             = 60,
    DXGI_FORMAT_R8_UNORM                                = 61,
    DXGI_FORMAT_R8_UINT                                 = 62,
    DXGI_FORMAT_R8_SNORM                                = 63,
    DXGI_FORMAT_R8_SINT                                 = 64,
    DXGI_FORMAT_A8_UNORM                                = 65,
    DXGI_FORMAT_R1_UNORM                                = 66,
    DXGI_FORMAT_R9G9B9E5_SHAREDEXP                      = 67,
    DXGI_FORMAT_R8G8_B8G8_UNORM                         = 68,
    DXGI_FORMAT_G8R8_G8B8_UNORM                         = 69,
    DXGI_FORMAT_BC1_TYPELESS                            = 70,
    DXGI_FORMAT_BC1_UNORM                               = 71,
    DXGI_FORMAT_BC1_UNORM_SRGB                          = 72,
    DXGI_FORMAT_BC2_TYPELESS                            = 73,
    DXGI_FORMAT_BC2_UNORM                               = 74,
    DXGI_FORMAT_BC2_UNORM_SRGB                          = 75,
    DXGI_FORMAT_BC3_TYPELESS                            = 76,
    DXGI_FORMAT_BC3_UNORM                               = 77,
    DXGI_FORMAT_BC3_UNORM_SRGB                          = 78,
    DXGI_FORMAT_BC4_TYPELESS                            = 79,
    DXGI_FORMAT_BC4_UNORM                               = 80,
    DXGI_FORMAT_BC4_SNORM                               = 81,
    DXGI_FORMAT_BC5_TYPELESS                            = 82,
    DXGI_FORMAT_BC5_UNORM                               = 83,
    DXGI_FORMAT_BC5_SNORM                               = 84,
    DXGI_FORMAT_B5G6R5_UNORM                            = 85,
    DXGI_FORMAT_B5G5R5A1_UNORM                          = 86,
    DXGI_FORMAT_B8G8R8A8_UNORM                          = 87,
    DXGI_FORMAT_B8G8R8X8_UNORM                          = 88,
    DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM              = 89,
    DXGI_FORMAT_B8G8R8A8_TYPELESS                       = 90,
    DXGI_FORMAT_B8G8R8A8_UNORM_SRGB                     = 91,
    DXGI_FORMAT_B8G8R8X8_TYPELESS                       = 92,
    DXGI_FORMAT_B8G8R8X8_UNORM_SRGB                     = 93,
    DXGI_FORMAT_BC6H_TYPELESS                           = 94,
    DXGI_FORMAT_BC6H_UF16                               = 95,
    DXGI_FORMAT_BC6H_SF16                               = 96,
    DXGI_FORMAT_BC7_TYPELESS                            = 97,
    DXGI_FORMAT_BC7_UNORM                               = 98,
    DXGI_FORMAT_BC7_UNORM_SRGB                          = 99,
    DXGI_FORMAT_AYUV                                    = 100,
    DXGI_FORMAT_Y410                                    = 101,
    DXGI_FORMAT_Y416                                    = 102,
    DXGI_FORMAT_NV12                                    = 103,
    DXGI_FORMAT_P010                                    = 104,
    DXGI_FORMAT_P016                                    = 105,
    DXGI_FORMAT_420_OPAQUE                              = 106,
    DXGI_FORMAT_YUY2                                    = 107,
    DXGI_FORMAT_Y210                                    = 108,
    DXGI_FORMAT_Y216                                    = 109,
    DXGI_FORMAT_NV11                                    = 110,
    DXGI_FORMAT_AI44                                    = 111,
    DXGI_FORMAT_IA44                                    = 112,
    DXGI_FORMAT_P8                                      = 113,
    DXGI_FORMAT_A8P8                                    = 114,
    DXGI_FORMAT_B4G4R4A4_UNORM                          = 115,

    DXGI_FORMAT_P208                                    = 130,
    DXGI_FORMAT_V208                                    = 131,
    DXGI_FORMAT_V408                                    = 132,


    DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE         = 189,
    DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE = 190,


    DXGI_FORMAT_FORCE_UINT                              = 0xffffffff
}

public class DXGI_SAMPLE_DESC : D3D11_Content
{
    public uint Count;
    public uint Quality;
}

public enum D3D11_USAGE : uint
{
    D3D11_USAGE_DEFAULT = 0,
    D3D11_USAGE_IMMUTABLE = 1,
    D3D11_USAGE_DYNAMIC = 2,
    D3D11_USAGE_STAGING = 3
}

public class D3D11_SUBRESOURCE_DATA : D3D11_Content
{
    public byte[] pSysMem;
    public uint SysMemPitch;
    public uint SysMemSlicePitch;

    // pSysMem数据在文件数据中的偏移和长度，由于并不真正读取pSysMem，所以需要记录原始数据信息
    public int sysMemDataOffset;
    public int sysMemLength;
}

public class D3D11_SHADER_RESOURCE_VIEW_DESC : D3D11_Content
{
    public DXGI_FORMAT Format;
    public D3D_SRV_DIMENSION ViewDimension;

    // 以下N选1，对应 union
    public D3D11_BUFFER_SRV Buffer;
    public D3D11_TEX1D_SRV Texture1D;
    public D3D11_TEX1D_ARRAY_SRV Texture1DArray;
    public D3D11_TEX2D_SRV Texture2D;
    public D3D11_TEX2D_ARRAY_SRV Texture2DArray;
    public D3D11_TEX2DMS_SRV Texture2DMS;
    public D3D11_TEX2DMS_ARRAY_SRV Texture2DMSArray;
    public D3D11_TEX3D_SRV Texture3D;
    public D3D11_TEXCUBE_SRV TextureCube;
    public D3D11_TEXCUBE_ARRAY_SRV TextureCubeArray;
    public D3D11_BUFFEREX_SRV BufferEx;

}

public enum D3D_SRV_DIMENSION
{
    D3D_SRV_DIMENSION_UNKNOWN = 0,
    D3D_SRV_DIMENSION_BUFFER = 1,
    D3D_SRV_DIMENSION_TEXTURE1D = 2,
    D3D_SRV_DIMENSION_TEXTURE1DARRAY = 3,
    D3D_SRV_DIMENSION_TEXTURE2D = 4,
    D3D_SRV_DIMENSION_TEXTURE2DARRAY = 5,
    D3D_SRV_DIMENSION_TEXTURE2DMS = 6,
    D3D_SRV_DIMENSION_TEXTURE2DMSARRAY = 7,
    D3D_SRV_DIMENSION_TEXTURE3D = 8,
    D3D_SRV_DIMENSION_TEXTURECUBE = 9,
    D3D_SRV_DIMENSION_TEXTURECUBEARRAY = 10,
    D3D_SRV_DIMENSION_BUFFEREX = 11,
    D3D10_SRV_DIMENSION_UNKNOWN = D3D_SRV_DIMENSION_UNKNOWN,
    D3D10_SRV_DIMENSION_BUFFER = D3D_SRV_DIMENSION_BUFFER,
    D3D10_SRV_DIMENSION_TEXTURE1D = D3D_SRV_DIMENSION_TEXTURE1D,
    D3D10_SRV_DIMENSION_TEXTURE1DARRAY = D3D_SRV_DIMENSION_TEXTURE1DARRAY,
    D3D10_SRV_DIMENSION_TEXTURE2D = D3D_SRV_DIMENSION_TEXTURE2D,
    D3D10_SRV_DIMENSION_TEXTURE2DARRAY = D3D_SRV_DIMENSION_TEXTURE2DARRAY,
    D3D10_SRV_DIMENSION_TEXTURE2DMS = D3D_SRV_DIMENSION_TEXTURE2DMS,
    D3D10_SRV_DIMENSION_TEXTURE2DMSARRAY = D3D_SRV_DIMENSION_TEXTURE2DMSARRAY,
    D3D10_SRV_DIMENSION_TEXTURE3D = D3D_SRV_DIMENSION_TEXTURE3D,
    D3D10_SRV_DIMENSION_TEXTURECUBE = D3D_SRV_DIMENSION_TEXTURECUBE,
    D3D10_1_SRV_DIMENSION_UNKNOWN = D3D_SRV_DIMENSION_UNKNOWN,
    D3D10_1_SRV_DIMENSION_BUFFER = D3D_SRV_DIMENSION_BUFFER,
    D3D10_1_SRV_DIMENSION_TEXTURE1D = D3D_SRV_DIMENSION_TEXTURE1D,
    D3D10_1_SRV_DIMENSION_TEXTURE1DARRAY = D3D_SRV_DIMENSION_TEXTURE1DARRAY,
    D3D10_1_SRV_DIMENSION_TEXTURE2D = D3D_SRV_DIMENSION_TEXTURE2D,
    D3D10_1_SRV_DIMENSION_TEXTURE2DARRAY = D3D_SRV_DIMENSION_TEXTURE2DARRAY,
    D3D10_1_SRV_DIMENSION_TEXTURE2DMS = D3D_SRV_DIMENSION_TEXTURE2DMS,
    D3D10_1_SRV_DIMENSION_TEXTURE2DMSARRAY = D3D_SRV_DIMENSION_TEXTURE2DMSARRAY,
    D3D10_1_SRV_DIMENSION_TEXTURE3D = D3D_SRV_DIMENSION_TEXTURE3D,
    D3D10_1_SRV_DIMENSION_TEXTURECUBE = D3D_SRV_DIMENSION_TEXTURECUBE,
    D3D10_1_SRV_DIMENSION_TEXTURECUBEARRAY = D3D_SRV_DIMENSION_TEXTURECUBEARRAY,
    D3D11_SRV_DIMENSION_UNKNOWN = D3D_SRV_DIMENSION_UNKNOWN,
    D3D11_SRV_DIMENSION_BUFFER = D3D_SRV_DIMENSION_BUFFER,
    D3D11_SRV_DIMENSION_TEXTURE1D = D3D_SRV_DIMENSION_TEXTURE1D,
    D3D11_SRV_DIMENSION_TEXTURE1DARRAY = D3D_SRV_DIMENSION_TEXTURE1DARRAY,
    D3D11_SRV_DIMENSION_TEXTURE2D = D3D_SRV_DIMENSION_TEXTURE2D,
    D3D11_SRV_DIMENSION_TEXTURE2DARRAY = D3D_SRV_DIMENSION_TEXTURE2DARRAY,
    D3D11_SRV_DIMENSION_TEXTURE2DMS = D3D_SRV_DIMENSION_TEXTURE2DMS,
    D3D11_SRV_DIMENSION_TEXTURE2DMSARRAY = D3D_SRV_DIMENSION_TEXTURE2DMSARRAY,
    D3D11_SRV_DIMENSION_TEXTURE3D = D3D_SRV_DIMENSION_TEXTURE3D,
    D3D11_SRV_DIMENSION_TEXTURECUBE = D3D_SRV_DIMENSION_TEXTURECUBE,
    D3D11_SRV_DIMENSION_TEXTURECUBEARRAY = D3D_SRV_DIMENSION_TEXTURECUBEARRAY,
    D3D11_SRV_DIMENSION_BUFFEREX = D3D_SRV_DIMENSION_BUFFEREX
}

public enum D3D11_RTV_DIMENSION
{
    D3D11_RTV_DIMENSION_UNKNOWN = 0,
    D3D11_RTV_DIMENSION_BUFFER = 1,
    D3D11_RTV_DIMENSION_TEXTURE1D = 2,
    D3D11_RTV_DIMENSION_TEXTURE1DARRAY = 3,
    D3D11_RTV_DIMENSION_TEXTURE2D = 4,
    D3D11_RTV_DIMENSION_TEXTURE2DARRAY = 5,
    D3D11_RTV_DIMENSION_TEXTURE2DMS = 6,
    D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY = 7,
    D3D11_RTV_DIMENSION_TEXTURE3D = 8
}

public class D3D11_BUFFER_SRV : D3D11_Content
{
    // 2选1
    public uint FirstElement;
    public uint ElementOffset;

    // 2选1
    public uint NumElements;
    public uint ElementWidth;
}

public class D3D11_TEX1D_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
}

public class D3D11_TEX1D_ARRAY_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2D_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
}

public class D3D11_TEX2D_ARRAY_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2DMS_SRV : D3D11_Content
{
    public uint UnusedField_NothingToDefine;
}

public class D3D11_TEX2DMS_ARRAY_SRV : D3D11_Content
{
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX3D_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
}

public class D3D11_TEXCUBE_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
}

public class D3D11_TEXCUBE_ARRAY_SRV : D3D11_Content
{
    public uint MostDetailedMip;
    public uint MipLevels;
    public uint First2DArrayFace;
    public uint NumCubes;
}

public class D3D11_BUFFEREX_SRV : D3D11_Content
{
    public uint FirstElement;
    public uint NumElements;
    public uint Flags;
}

public class D3D11_BUFFER_RTV : D3D11_Content
{
    // 2选1
    public uint FirstElement;
    public uint ElementOffset;

    // 2选1
    public uint NumElements;
    public uint ElementWidth;
}

public class D3D11_TEX1D_RTV : D3D11_Content
{
    public uint MipSlice;
}

public class D3D11_TEX1D_ARRAY_RTV : D3D11_Content
{
    public uint MipSlice;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2D_RTV : D3D11_Content
{
    public uint MipSlice;
}

public class D3D11_TEX2D_ARRAY_RTV : D3D11_Content
{
    public uint MipSlice;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2DMS_RTV : D3D11_Content
{
    public uint UnusedField_NothingToDefine;
}

public class D3D11_TEX2DMS_ARRAY_RTV : D3D11_Content
{
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX3D_RTV : D3D11_Content
{
    public uint MipSlice;
    public uint FirstWSlice;
    public uint WSize;
}

public enum D3D11_DSV_DIMENSION
{
    D3D11_DSV_DIMENSION_UNKNOWN	= 0,
    D3D11_DSV_DIMENSION_TEXTURE1D = 1,
    D3D11_DSV_DIMENSION_TEXTURE1DARRAY = 2,
    D3D11_DSV_DIMENSION_TEXTURE2D = 3,
    D3D11_DSV_DIMENSION_TEXTURE2DARRAY = 4,
    D3D11_DSV_DIMENSION_TEXTURE2DMS = 5,
    D3D11_DSV_DIMENSION_TEXTURE2DMSARRAY = 6
}

public class D3D11_TEX1D_DSV : D3D11_Content
{
    public uint MipSlice;
}

public class D3D11_TEX1D_ARRAY_DSV : D3D11_Content
{
    public uint MipSlice;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2D_DSV : D3D11_Content
{
    public uint MipSlice;
}

public class D3D11_TEX2D_ARRAY_DSV : D3D11_Content
{
    public uint MipSlice;
    public uint FirstArraySlice;
    public uint ArraySize;
}

public class D3D11_TEX2DMS_DSV : D3D11_Content
{
    public uint UnusedField_NothingToDefine;
}

public class D3D11_TEX2DMS_ARRAY_DSV : D3D11_Content
{
    public uint FirstArraySlice;
    public uint ArraySize;
}

public enum D3D11_DSV_FLAG
{
    D3D11_DSV_READ_ONLY_DEPTH = 0x1,
    D3D11_DSV_READ_ONLY_STENCIL = 0x2
}

public class D3D11_BUFFER_DESC : D3D11_Content
{
    public uint ByteWidth;
    public D3D11_USAGE Usage;
    public uint BindFlags;
    public uint CPUAccessFlags;
    public uint MiscFlags;
    public uint StructureByteStride;
}