using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdc
{
    /// <summary>
    /// 从官方renderdoc代码中复制过来的通用函数
    /// </summary>
    public class Common
    {
        public static uint CalcNumMips(uint w, uint h, uint d)
        {
            uint mipLevels = 1;

            while (w > 1 || h > 1 || d > 1)
            {
                w = Math.Max(1, w >> 1);
                h = Math.Max(1, h >> 1);
                d = Math.Max(1, d >> 1);
                mipLevels++;
            }

            return mipLevels;
        }

        public static uint GetByteSize(int Width, int Height, int Depth, DXGI_FORMAT Format, int mip)
        {
            uint ret = (uint)(Math.Max(Width >> mip, 1) * Math.Max(Height >> mip, 1) * Math.Max(Depth >> mip, 1));

            switch (Format)
            {
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT: ret *= 16; break;
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_SINT: ret *= 12; break;
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G8X24_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT: ret *= 8; break;
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R24G8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB: ret *= 4; break;
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_D16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM: ret *= 2; break;
                case DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM: ret *= 1; break;
                case DXGI_FORMAT.DXGI_FORMAT_R1_UNORM: ret = Math.Max(ret / 8, 1U); break;
                case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                    ret = (uint)(AlignUp4(Math.Max(Width >> mip, 1)) * AlignUp4(Math.Max(Height >> mip, 1)) *
                          Math.Max(Depth >> mip, 1));
                    ret /= 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                    ret = (uint)(AlignUp4(Math.Max(Width >> mip, 1)) * AlignUp4(Math.Max(Height >> mip, 1)) *
                          Math.Max(Depth >> mip, 1));
                    ret *= 1;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
                    ret *= 2;    // 4 channels, half a byte each
                    break;
                /*
                 * YUV planar/packed subsampled textures.
                 *
                 * In each diagram we indicate (maybe part) of the data for a 4x4 texture:
                 *
                 * +---+---+---+---+
                 * | 0 | 1 | 2 | 3 |
                 * +---+---+---+---+
                 * | 4 | 5 | 6 | 7 |
                 * +---+---+---+---+
                 * | 8 | 9 | A | B |
                 * +---+---+---+---+
                 * | C | D | E | F |
                 * +---+---+---+---+
                 *
                 *
                 * FOURCC decoding:
                 *  - char 0: 'Y' = packed, 'P' = planar
                 *  - char 1: '4' = 4:4:4, '2' = 4:2:2, '1' = 4:2:1, '0' = 4:2:0
                 *  - char 2+3: '16' = 16-bit, '10' = 10-bit, '08' = 8-bit
                 *
                 * planar = Y is first, all together, then UV comes second.
                 * packed = YUV is interleaved
                 *
                 * ======================= 4:4:4 lossless packed =========================
                 *
                 * Equivalent to uncompressed formats, just YUV instead of RGB. For 8-bit:
                 *
                 * pixel:      0            1            2            3
                 * byte:  0  1  2  3   4  5  6  7   8  9  A  B   C  D  E  F
                 *        Y0 U0 V0 A0  Y1 U1 V1 A1  Y2 U2 V2 A2  Y3 U3 V3 A3
                 *
                 * 16-bit is similar with two bytes per sample, 10-bit for uncompressed is
                 * equivalent to R10G10B10A2 but with RGB=>YUV
                 *
                 * ============================ 4:2:2 packed =============================
                 *
                 * 50% horizontal subsampling packed, two Y samples for each U/V sample pair. For 8-bit:
                 *
                 * pixel:   0  |  1      2  |  3      4  |  5      6  |  7
                 * byte:  0  1  2  3   4  5  6  7   8  9  A  B   C  D  E  F
                 *        Y0 U0 Y1 V0  Y2 U1 Y3 V1  Y4 U2 Y5 V2  Y6 U3 Y7 V3
                 *
                 * 16-bit is similar with two bytes per sample, 10-bit is stored identically to 16-bit but in
                 * the most significant bits:
                 *
                 * bit:    FEDCBA9876543210
                 * 16-bit: XXXXXXXXXXXXXXXX
                 * 10-bit: XXXXXXXXXX000000
                 *
                 * Since the data is unorm this just spaces out valid values.
                 *
                 * ============================ 4:2:0 planar =============================
                 *
                 * 50% horizontal and vertical subsampled planar, four Y samples for each U/V sample pair.
                 * For 8-bit:
                 *
                 *
                 * pixel: 0  1  2  3   4  5  6  7
                 * byte:  0  1  2  3   4  5  6  7
                 *        Y0 Y1 Y2 Y3  Y4 Y5 Y6 Y7
                 *
                 * pixel: 8  9  A  B   C  D  E  F
                 * byte:  8  9  A  B   C  D  E  F
                 *        Y8 Y9 Ya Yb  Yc Yd Ye Yf
                 *
                 *        ... all of the rest of Y luma ...
                 *
                 * pixel:  T&4 | 1&5    2&6 | 3&7
                 * byte:  0  1  2  3   4  5  6  7
                 *        U0 V0 U1 V1  U2 V2 U3 V3
                 *
                 * pixel:  8&C | 9&D    A&E | B&F
                 * byte:  8  9  A  B   C  D  E  F
                 *        U4 V4 U5 V5  U6 V6 U7 V7
                 */
                case DXGI_FORMAT.DXGI_FORMAT_AYUV:
                    // 4:4:4 lossless packed, 8-bit. Equivalent size to R8G8B8A8
                    ret *= 4;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_Y410:
                    // 4:4:4 lossless packed. Equivalent size to R10G10B10A2, unlike most 10-bit/16-bit formats is
                    // not equivalent to the 16-bit format.
                    ret *= 4;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_Y416:
                    // 4:4:4 lossless packed. Equivalent size to R16G16B16A16
                    ret *= 8;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_NV12:
                    // 4:2:0 planar. Since we can assume even width and height, resulting size is 1 byte per pixel
                    // for luma, plus 1 byte per 2 pixels for chroma
                    ret = ret + ret / 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_P010:
                    // 10-bit formats are stored identically to 16-bit formats
                    //DELIBERATE_FALLTHROUGH();
                case DXGI_FORMAT.DXGI_FORMAT_P016:
                    // 4:2:0 planar but 16-bit, so pixelCount*2 + (pixelCount*2) / 2
                    ret *= 2;
                    ret = ret + ret / 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                    // same size as NV12 - planar 4:2:0 but opaque layout
                    ret = ret + ret / 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_YUY2:
                    // 4:2:2 packed 8-bit, so 1 byte per pixel for luma and 1 byte per pixel for chroma (2 chroma
                    // samples, with 50% subsampling = 1 byte per pixel)
                    ret *= 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_Y210:
                    // 10-bit formats are stored identically to 16-bit formats
                    //DELIBERATE_FALLTHROUGH();
                case DXGI_FORMAT.DXGI_FORMAT_Y216:
                    // 4:2:2 packed 16-bit
                    ret *= 4;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_NV11:
                    // similar to NV11 - planar 4:1:1 4 horizontal downsampling but no vertical downsampling. For
                    // size calculation amounts to the same result.
                    ret = ret + ret / 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_AI44:
                    // special format, 1 byte per pixel, palletised values in 4 most significant bits, alpha in 4
                    // least significant bits.
                    //DELIBERATE_FALLTHROUGH();
                case DXGI_FORMAT.DXGI_FORMAT_IA44:
                    // same as above but swapped MSB/LSB
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_P8:
                    // 8 bits of palletised data
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_A8P8:
                    // 8 bits palletised data, 8 bits alpha data. Seems to be packed (no indication in docs of
                    // planar)
                    ret *= 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_P208:
                    // 4:2:2 planar 8-bit. 1 byte per pixel of luma, then separately 1 byte per pixel of chroma.
                    // Identical size to packed 4:2:2, just different layout
                    ret *= 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_V208:
                    // unclear, seems to be packed 4:4:0 8-bit. Thus 1 byte per pixel for luma, 2 chroma samples
                    // every 2 rows = 1 byte per pixel for chroma
                    ret *= 2;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_V408:
                    // unclear, seems to be packed 4:4:4 8-bit
                    ret *= 4;
                    break;

                case DXGI_FORMAT.DXGI_FORMAT_UNKNOWN:
                    Console.WriteLine("Getting byte size of unknown DXGI format");
                    ret = 0;
                    break;
                default: Console.WriteLine("Unrecognised DXGI Format: %d", Format); break;
            }

            return ret;
        }

        public static uint AlignUp4(uint x)
        {
            return (uint)((x + 0x3) & (~0x3));
        }

        public static long AlignUp4(long x)
        {
            return (x + 0x3) & (~0x3);
        }

        public static bool IsBlockFormat(DXGI_FORMAT f)
        {
            switch (f)
            {
                case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB: return true;
                default: break;
            }

            return false;
        }

        public static bool IsYUVPlanarFormat(DXGI_FORMAT f)
        {
            switch (f)
            {
                case DXGI_FORMAT.DXGI_FORMAT_NV12:
                case DXGI_FORMAT.DXGI_FORMAT_P010:
                case DXGI_FORMAT.DXGI_FORMAT_P016:
                case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                case DXGI_FORMAT.DXGI_FORMAT_NV11:
                case DXGI_FORMAT.DXGI_FORMAT_P208: return true;
                default: break;
            }

            return false;
        }

        public static uint GetYUVNumRows(DXGI_FORMAT f, uint height)
        {
            switch (f)
            {
                case DXGI_FORMAT.DXGI_FORMAT_NV12:
                case DXGI_FORMAT.DXGI_FORMAT_P010:
                case DXGI_FORMAT.DXGI_FORMAT_P016:
                case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                    // all of these are 4:2:0, so number of rows is equal to height + height/2
                    return height + height / 2;
                case DXGI_FORMAT.DXGI_FORMAT_NV11:
                case DXGI_FORMAT.DXGI_FORMAT_P208:
                    // 4:1:1 and 4:2:2 have the same number of rows for chroma and luma planes, so we have
                    // height * 2 rows
                    return height * 2;
                default: break;
            }

            return height;
        }
    }
}
