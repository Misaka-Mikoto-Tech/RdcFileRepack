using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// D3D11 相关结构体的Reader
/// </summary>
public unsafe static class D3D11Reader
{
    /// <summary>
    /// 由于C#没有偏特化，因此使用手动注册回调的方式实现泛型读取器的重用
    /// </summary>
    private static Dictionary<Type, Func<BinaryReader, object>> _dicReadFuncs = new Dictionary<Type, Func<BinaryReader, object>>();

    static D3D11Reader()
    {
        _dicReadFuncs.Add(typeof(D3D11_TEXTURE2D_DESC), Read_D3D11_TEXTURE2D_DESC);
        _dicReadFuncs.Add(typeof(D3D11_SUBRESOURCE_DATA), Read_D3D11_SUBRESOURCE_DATA);
        _dicReadFuncs.Add(typeof(D3D11_SHADER_RESOURCE_VIEW_DESC), Read_D3D11_SHADER_RESOURCE_VIEW_DESC);
        _dicReadFuncs.Add(typeof(D3D11_RENDER_TARGET_VIEW_DESC), Read_D3D11_RENDER_TARGET_VIEW_DESC);
        _dicReadFuncs.Add(typeof(D3D11_DEPTH_STENCIL_VIEW_DESC), Read_D3D11_DEPTH_STENCIL_VIEW_DESC);
        _dicReadFuncs.Add(typeof(D3D11_BUFFER_DESC), Read_D3D11_BUFFER_DESC);
        _dicReadFuncs.Add(typeof(D3D11InitParams), Read_D3D11InitParams);
    }

    public static object Read_D3D11_TEXTURE2D_DESC(BinaryReader br)
    {
        D3D11_TEXTURE2D_DESC desc = new D3D11_TEXTURE2D_DESC();
        desc.offset = br.BaseStream.Position;

        desc.Width = br.ReadUInt32();
        desc.Height = br.ReadUInt32();
        desc.MipLevels = br.ReadUInt32();
        desc.ArraySize = br.ReadUInt32();
        desc.Format = (DXGI_FORMAT)br.ReadUInt32();
        desc.SampleDesc = new DXGI_SAMPLE_DESC();
        desc.SampleDesc.Count = br.ReadUInt32();
        desc.SampleDesc.Quality = br.ReadUInt32();
        desc.Usage = (D3D11_USAGE)br.ReadUInt32();
        desc.BindFlags = br.ReadUInt32();
        desc.CPUAccessFlags = br.ReadUInt32();
        desc.MiscFlags = br.ReadUInt32();

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }

    public static object Read_D3D11_BUFFER_DESC(BinaryReader br)
    {
        D3D11_BUFFER_DESC desc = new D3D11_BUFFER_DESC();
        desc.offset = br.BaseStream.Position;

        desc.ByteWidth = br.ReadUInt32();
        desc.Usage = (D3D11_USAGE)br.ReadUInt32();
        desc.BindFlags = br.ReadUInt32();
        desc.CPUAccessFlags = br.ReadUInt32();
        desc.MiscFlags = br.ReadUInt32();
        desc.StructureByteStride = br.ReadUInt32();

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }

    public static object Read_D3D11_SHADER_RESOURCE_VIEW_DESC(BinaryReader br)
    {
        D3D11_SHADER_RESOURCE_VIEW_DESC desc = new D3D11_SHADER_RESOURCE_VIEW_DESC();
        desc.offset = br.BaseStream.Position;

        desc.Format = (DXGI_FORMAT)br.ReadUInt32();
        desc.ViewDimension = (D3D_SRV_DIMENSION)br.ReadInt32();

        switch(desc.ViewDimension)
        {
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_UNKNOWN:
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_BUFFER:
                desc.Buffer = new D3D11_BUFFER_SRV();
                desc.Buffer.FirstElement = br.ReadUInt32();
                desc.Buffer.NumElements = br.ReadUInt32();

                desc.Buffer.ElementOffset = desc.Buffer.FirstElement;
                desc.Buffer.ElementWidth = desc.Buffer.NumElements;
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE1D:
                desc.Texture1D = new D3D11_TEX1D_SRV();
                desc.Texture1D.MostDetailedMip = br.ReadUInt32();
                desc.Texture1D.MipLevels = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE1DARRAY:
                desc.Texture1DArray = new D3D11_TEX1D_ARRAY_SRV();
                desc.Texture1DArray.MostDetailedMip = br.ReadUInt32();
                desc.Texture1DArray.MipLevels = br.ReadUInt32();
                desc.Texture1DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture1DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE2D:
                desc.Texture2D = new D3D11_TEX2D_SRV();
                desc.Texture2D.MostDetailedMip = br.ReadUInt32();
                desc.Texture2D.MipLevels = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE2DARRAY:
                desc.Texture2DArray = new D3D11_TEX2D_ARRAY_SRV();
                desc.Texture2DArray.MostDetailedMip = br.ReadUInt32();
                desc.Texture2DArray.MipLevels = br.ReadUInt32();
                desc.Texture2DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE2DMS:
                desc.Texture2DMS = new D3D11_TEX2DMS_SRV();
                desc.Texture2DMS.UnusedField_NothingToDefine = 0; // dummy
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE2DMSARRAY:
                desc.Texture2DMSArray = new D3D11_TEX2DMS_ARRAY_SRV();
                desc.Texture2DMSArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DMSArray.ArraySize = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURE3D:
                desc.Texture3D = new D3D11_TEX3D_SRV();
                desc.Texture3D.MostDetailedMip = br.ReadUInt32();
                desc.Texture3D.MipLevels = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURECUBE:
                desc.TextureCube = new D3D11_TEXCUBE_SRV();
                desc.TextureCube.MostDetailedMip = br.ReadUInt32();
                desc.TextureCube.MipLevels = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_TEXTURECUBEARRAY:
                desc.TextureCubeArray = new D3D11_TEXCUBE_ARRAY_SRV();
                desc.TextureCubeArray.MostDetailedMip = br.ReadUInt32();
                desc.TextureCubeArray.MipLevels = br.ReadUInt32();
                desc.TextureCubeArray.First2DArrayFace = br.ReadUInt32();
                desc.TextureCubeArray.NumCubes = br.ReadUInt32();
                break;
            case D3D_SRV_DIMENSION.D3D11_SRV_DIMENSION_BUFFEREX:
                desc.BufferEx = new D3D11_BUFFEREX_SRV();
                desc.BufferEx.FirstElement = br.ReadUInt32();
                desc.BufferEx.NumElements = br.ReadUInt32();
                desc.BufferEx.Flags = br.ReadUInt32();
                break;
            default:
                throw new Exception($"Unrecognised SRV Dimension {desc.ViewDimension}");

        }

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }

    public static object Read_D3D11_RENDER_TARGET_VIEW_DESC(BinaryReader br)
    {
        D3D11_RENDER_TARGET_VIEW_DESC desc = new D3D11_RENDER_TARGET_VIEW_DESC();
        desc.offset = br.BaseStream.Position;

        desc.Format = (DXGI_FORMAT)br.ReadUInt32();
        desc.ViewDimension = (D3D11_RTV_DIMENSION)br.ReadUInt32();

        switch(desc.ViewDimension)
        {
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_UNKNOWN:
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_BUFFER:
                desc.Buffer = new D3D11_BUFFER_RTV();
                desc.Buffer.FirstElement = br.ReadUInt32();
                desc.Buffer.NumElements = br.ReadUInt32();

                desc.Buffer.ElementOffset = desc.Buffer.FirstElement;
                desc.Buffer.ElementWidth = desc.Buffer.NumElements;
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE1D:
                desc.Texture1D = new D3D11_TEX1D_RTV();
                desc.Texture1D.MipSlice = br.ReadUInt32();
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE1DARRAY:
                desc.Texture1DArray = new D3D11_TEX1D_ARRAY_RTV();
                desc.Texture1DArray.MipSlice = br.ReadUInt32();
                desc.Texture1DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture1DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE2D:
                desc.Texture2D = new D3D11_TEX2D_RTV();
                desc.Texture2D.MipSlice = br.ReadUInt32();
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE2DARRAY:
                desc.Texture2DArray = new D3D11_TEX2D_ARRAY_RTV();
                desc.Texture2DArray.MipSlice = br.ReadUInt32();
                desc.Texture2DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE2DMS:
                desc.Texture2DMS = new D3D11_TEX2DMS_RTV();
                desc.Texture2DMS.UnusedField_NothingToDefine = 0; // dummy
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY:
                desc.Texture2DMSArray = new D3D11_TEX2DMS_ARRAY_RTV();
                desc.Texture2DMSArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DMSArray.ArraySize = br.ReadUInt32();
                break;
            case D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_TEXTURE3D:
                desc.Texture3D = new D3D11_TEX3D_RTV();
                desc.Texture3D.MipSlice = br.ReadUInt32();
                desc.Texture3D.FirstWSlice = br.ReadUInt32();
                desc.Texture3D.WSize = br.ReadUInt32();
                break;
            default:
                throw new Exception($"Unrecognised RTV Dimension {desc.ViewDimension}");
        }

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }

    public static object Read_D3D11_DEPTH_STENCIL_VIEW_DESC(BinaryReader br)
    {
        D3D11_DEPTH_STENCIL_VIEW_DESC desc = new D3D11_DEPTH_STENCIL_VIEW_DESC();
        desc.offset = br.BaseStream.Position;

        desc.Format = (DXGI_FORMAT)br.ReadUInt32();
        desc.ViewDimension = (D3D11_DSV_DIMENSION)br.ReadUInt32();
        desc.Flags = (D3D11_DSV_FLAG)br.ReadUInt32();

        switch(desc.ViewDimension)
        {
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_UNKNOWN:
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE1D:
                desc.Texture1D = new D3D11_TEX1D_DSV();
                desc.Texture1D.MipSlice = br.ReadUInt32();
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE1DARRAY:
                desc.Texture1DArray = new D3D11_TEX1D_ARRAY_DSV();
                desc.Texture1DArray.MipSlice = br.ReadUInt32();
                desc.Texture1DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture1DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE2D:
                desc.Texture2D = new D3D11_TEX2D_DSV();
                desc.Texture2D.MipSlice = br.ReadUInt32();
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE2DARRAY:
                desc.Texture2DArray = new D3D11_TEX2D_ARRAY_DSV();
                desc.Texture2DArray.MipSlice = br.ReadUInt32();
                desc.Texture2DArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DArray.ArraySize = br.ReadUInt32();
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE2DMS:
                desc.Texture2DMS = new D3D11_TEX2DMS_DSV();
                desc.Texture2DMS.UnusedField_NothingToDefine = 0; //dummy
                break;
            case D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE2DMSARRAY:
                desc.Texture2DMSArray = new D3D11_TEX2DMS_ARRAY_DSV();
                desc.Texture2DMSArray.FirstArraySlice = br.ReadUInt32();
                desc.Texture2DMSArray.ArraySize = br.ReadUInt32();
                break;
            default:
                throw new Exception($"Unrecognised DSV Dimension {desc.ViewDimension}");
        }

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }

    /// <summary>
    /// 首次初始化 D3D11_SUBRESOURCE_DATA， 此结构需要分两次读取，第二次会填充pSysMem
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static object Read_D3D11_SUBRESOURCE_DATA(BinaryReader br)
    {
        D3D11_SUBRESOURCE_DATA data = new D3D11_SUBRESOURCE_DATA();
        data.offset = br.BaseStream.Position;

        // don't serialise pSysMem, just set it to NULL. See the definition of SERIALISE_MEMBER_DUMMY
        data.pSysMem = Read_BytesArray(br, true);
        data.SysMemPitch = br.ReadUInt32();
        data.SysMemSlicePitch = br.ReadUInt32();

        data.length = br.BaseStream.Position - data.offset;
        return data;
    }

    /// <summary>
    /// 读取首个chunk(DriverInit)时会用到，保存了DeviceName等信息
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static object Read_D3D11InitParams(BinaryReader br)
    {
        D3D11InitParams initParams = new D3D11InitParams();
        initParams.offset = br.BaseStream.Position;

        initParams.DriverType = (D3D_DRIVER_TYPE)br.ReadInt32();
        initParams.Flags = br.ReadUInt32();
        initParams.SDKVersion = br.ReadUInt32();
        initParams.NumFeatureLevels = br.ReadUInt32();
        initParams.FeatureLevels = Read_Primitive_Array<D3D_FEATURE_LEVEL>(br);
        initParams.AdapterDesc = Read_DXGI_ADAPTER_DESC(br) as DXGI_ADAPTER_DESC;

        initParams.length = br.BaseStream.Position - initParams.offset;
        return initParams;
    }

    public static object Read_DXGI_ADAPTER_DESC(BinaryReader br)
    {
        DXGI_ADAPTER_DESC desc = new DXGI_ADAPTER_DESC();
        desc.offset = br.BaseStream.Position;

        desc.Description = Utils.ReadChunkString(br);
        desc.DescriptionLen = desc.Description.Length;
        desc.VendorId = br.ReadUInt32();
        desc.DeviceId = br.ReadUInt32();
        desc.SubSysId = br.ReadUInt32();
        desc.Revision = br.ReadUInt32();
        desc.DedicatedVideoMemory = br.ReadUInt64();
        desc.DedicatedSystemMemory = br.ReadUInt64();
        desc.SharedSystemMemory = br.ReadUInt64();
        desc.AdapterLuid.LowPart = br.ReadUInt32();
        desc.AdapterLuid.HighPart = br.ReadInt32();

        desc.length = br.BaseStream.Position - desc.offset;
        return desc;
    }


    #region 辅助函数
    /// <summary>
    /// 读取可空类型数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <returns></returns>
    public static T Read_D3D11_Nullable<T>(BinaryReader br) where T:class, new()
    {
        bool present = br.ReadBoolean();
        if (!present)
            return null;

        Func<BinaryReader, object> readFunc;
        if (!_dicReadFuncs.TryGetValue(typeof(T), out readFunc))
        {
            throw new Exception($"unexpected type {typeof(T).Name} when read");
        }

        T ret = readFunc(br) as T;
        return ret;
    }

    /// <summary>
    /// 读取基本类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <returns></returns>
    public static T Read_Primitive<T>(BinaryReader br) where T:unmanaged
    {
        Type t = typeof(T);
        if (t.IsEnum)
            t = t.GetEnumUnderlyingType();

        Debug.Assert(t.IsPrimitive);

        T tVal = default;
        byte[] buff = new byte[8];
        fixed(void * p = &buff[0])
        {
            if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(bool))
            {
                br.Read(buff, 0, 1);
                *(byte*)&tVal = *(byte *)p;
            }
            else if (t == typeof(short) || t == typeof(short))
            {
                br.Read(buff, 0, 2);
                *(short*)&tVal = *(short*)p;
            }
            else if(t == typeof(int) || t == typeof(uint))
            {
                br.Read(buff, 0, 4);
                *(int*)&tVal = *(int*)p;
            }
            else if(t == typeof(float))
            {
                br.Read(buff, 0, 4);
                *(float*)&tVal = *(float*)p;
            }
            else if (t == typeof(double))
            {
                br.Read(buff, 0, 8);
                *(double*)&tVal = *(double*)p;
            }
            else if(t == typeof(long) || t == typeof(ulong))
            {
                br.Read(buff, 0, 8);
                *(long*)&tVal = *(long*)p;
            }
            else
            {
                throw new Exception($"unsupported type {t.Name}");
            }
        }

        return tVal;
    }

    /// <summary>
    /// 读取基础类型数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <returns></returns>
    public static T[] Read_Primitive_Array<T>(BinaryReader br) where T:unmanaged
    {
        ulong arrayCount = br.ReadUInt64();
        if (arrayCount == 0)
            return null;

        T[] arr = new T[arrayCount];
        for (int i = 0; i < (int)arrayCount; i++)
        {
            arr[i] = Read_Primitive<T>(br);
        }

        return arr;
    }

    /// <summary>
    /// 读取数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <returns></returns>
    public static T[] Read_D3D11_Array<T>(BinaryReader br) where T:class, new()
    {
        Func<BinaryReader, object> readFunc;

        if (!_dicReadFuncs.TryGetValue(typeof(T), out readFunc))
        {
            throw new Exception($"unexpected type {typeof(T).Name} when read");
        }

        ulong arrayCount = br.ReadUInt64();
        if (arrayCount == 0)
            return null;

        T[] arr = new T[arrayCount];
        for (int i = 0; i < (int)arrayCount; i++)
        {
            T data = readFunc(br) as T;
            arr[i] = data;
        }

        return arr;
    }

    /// <summary>
    /// 读取可空数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <returns></returns>
    public static T[] Read_D3D11_Nullable_Array<T>(BinaryReader br) where T : class, new()
    {
        bool present = br.ReadBoolean();
        if (!present)
            return null;

        Func<BinaryReader, object> readFunc;
        if (!_dicReadFuncs.TryGetValue(typeof(T), out readFunc))
        {
            throw new Exception($"unexpected type {typeof(T).Name} when read");
        }

        ulong arrayCount = br.ReadUInt64();
        if (arrayCount == 0)
            return null;

        T[] arr = new T[arrayCount];
        for (int i = 0; i < (int)arrayCount; i++)
        {
            T data = readFunc(br) as T;
            arr[i] = data;
        }

        return arr;
    }

    public static byte[] Read_BytesArray(BinaryReader br, bool skip = false)
    {
        return Read_BytesArray(br, out int dataOffset, out int count, skip);
    }

    /// <summary>
    /// 读取byte[]的特化
    /// </summary>
    /// <param name="br"></param>
    /// <param name="skip">是否只跳过不真正读取</param>
    /// <returns></returns>
    public static byte[] Read_BytesArray(BinaryReader br, out int dataOffset, out int count, bool skip = false)
    {
        count = (int)br.ReadUInt64();
        br.AlignUp(64);
        
        dataOffset = (int)br.BaseStream.Position;

        if(skip)
        {
            br.Skip(count);
            return null;
        }
        else
        {
            byte[] arr = br.ReadBytes(count);
            return arr;
        }
        
    }

    #endregion

    /// <summary>
    /// 从流中读取数据填充 data 参数（D3D11_SUBRESOURCE_DATA初始化的第二步）
    /// </summary>
    /// <param name="br"></param>
    /// <param name="data"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="d"></param>
    /// <param name="fmt"></param>
    /// <param name="mips"></param>
    /// <param name="arr"></param>
    /// <param name="HasData"></param>
    public static void Read_CreateTextureData(BinaryReader br, D3D11_SUBRESOURCE_DATA[] data, uint w, uint h, uint d,
        DXGI_FORMAT fmt, uint mips, uint arr, bool HasData)
    {
        uint numSubresources = mips;
        uint numMips = mips;

        numSubresources *= arr;

        Debug.Assert(numSubresources >= data.Length);

        for(int i = 0; i < numSubresources; i++)
        {
            int mip = (int)(i % numMips);

            uint subresourceContentsLength = Rdc.Common.GetByteSize((int)w, (int)h, (int)d, fmt, mip);

            Debug.Assert(subresourceContentsLength > 0);

            data[i].pSysMem = Read_BytesArray(br, out data[i].sysMemDataOffset, out data[i].sysMemLength, true); // 没有必要真的读，数据都在 br.BaseStream里,需要访问时去那里找就可以了

            uint dataLenCheck = br.ReadUInt32();
            Debug.Assert(subresourceContentsLength == dataLenCheck && data[i].sysMemLength == dataLenCheck);
        }
    }
}
