using Rdc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;

public unsafe static class Utils
{
    public static string GetStringFromStream<T>(BinaryReader br) where T : unmanaged
    {
        int len;
        int lenSize = sizeof(T);
        switch(lenSize)
        {
            case 1:
                len = br.ReadByte();
                break;
            case 2:
                len = br.ReadUInt16();
                break;
            case 4:
                len = br.ReadInt32();
                break;
            default:
                throw new Exception($"invalid string size {lenSize}");
        }

        if (len == 0)
            throw new Exception("string length is invalid, must be at least 1 to contain NULL terminator");

        byte[] buff = new byte[len];
        br.Read(buff, 0, len);

        string ret = Encoding.UTF8.GetString(buff, 0, len - 1); // rdc 文件存储的字符串都以 \0 结尾
        return ret;
    }

    public static void WriteStringToStream<T>(string str, BinaryWriter bw) where T : unmanaged
    {
        int len = str.Length + 1; // zero endding
        int lenSize = sizeof(T);
        switch (lenSize)
        {
            case 1:
                bw.Write((byte)len);
                break;
            case 2:
                bw.Write((ushort)len);
                break;
            case 4:
                bw.Write(len);
                break;
            default:
                throw new Exception($"invalid string size {lenSize}");
        }

        byte[] buff = Encoding.UTF8.GetBytes(str);
        bw.Write(buff);
        bw.Write((byte)0); // zero endding
    }

    /// <summary>
    /// 读取chunk内的字符串，此类字符串不以0结尾
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static string ReadChunkString(BinaryReader br)
    {
        int len = br.ReadInt32();
        byte[] buff = new byte[len];
        br.Read(buff, 0, len);

        string ret = Encoding.UTF8.GetString(buff, 0, len);
        return ret;
    }

    /// <summary>
    /// 向流中写入chunk类型字符串
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="str"></param>
    /// <param name="fixSize">固定大小，无论如何都将写入此长度的数据</param>
    public static bool WriteChunkString(BinaryWriter bw, string str, int fixSize = 0)
    {
        byte[] buff;
        if (fixSize == 0)
            buff = Encoding.UTF8.GetBytes(str);
        else
        {
            buff = new byte[fixSize];
            int cnt = Encoding.UTF8.GetBytes(str, 0, str.Length, buff, 0);
            if(cnt <= 0)
            {
                Console.WriteLine($"允许的最大字节数为 {fixSize}");
                return false;
            }

            for(int i = cnt; i < fixSize; i++)
            {
                buff[i] = 0;
            }
        }

        bw.Write(buff.Length);
        bw.Write(buff, 0, buff.Length);
        return true;
    }

    /// <summary>
    /// 从流中读取数据到 arr 中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="br"></param>
    /// <param name="arr"></param>
    /// <param name="count"></param>
    /// <returns>读取的字节数</returns>
    public static int Read<T>(this BinaryReader br, T[] arr, int index, int count) where T : unmanaged
    {
        Debug.Assert(arr != null && arr.Length >= count);

        byte[] buff = new byte[count * sizeof(T)];
        br.Read(buff, 0, buff.Length);

        fixed(void * p1 = &buff[0])
        fixed(void * p2 = &arr[index])
        {
            T* pSrc = (T*)p1;
            T* pDst = (T*)p2;

            for (int i = 0; i < count; i++)
                *pDst++ = *pSrc++;
        }

        return buff.Length;
    }

    /// <summary>
    /// 将数组 arr 数据写入流中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bw"></param>
    /// <param name="arr"></param>
    /// <param name="count"></param>
    /// <returns>写入的字节数</returns>
    public static int Write<T>(this BinaryWriter bw, T[] arr, int index, int count) where T:unmanaged
    {
        Debug.Assert(arr != null && arr.Length >= count);

        byte[] buff = new byte[count * sizeof(T)];

        fixed (void* p1 = &arr[index])
        fixed (void* p2 = &buff[0])
        {
            T* pSrc = (T*)p1;
            T* pDst = (T*)p2;

            for (int i = 0; i < count; i++)
                *pDst++ = *pSrc++;
        }

        bw.Write(buff, 0, buff.Length);
        return buff.Length;
    }

    /// <summary>
    /// 从 byte[] 中查找 子 byte[]
    /// </summary>
    /// <param name="srcBytes"></param>
    /// <param name="searchBytes"></param>
    /// <param name="startIndex">起始索引</param>
    /// <returns></returns>
    public static int IndexOf(this byte[] srcBytes, byte[] searchBytes, int startIndex = 0)
    {
        if (srcBytes == null) { return -1; }
        if (searchBytes == null) { return -1; }
        if (srcBytes.Length == 0) { return -1; }
        if (searchBytes.Length == 0) { return -1; }
        if (srcBytes.Length < searchBytes.Length) { return -1; }
        for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
        {
            if (srcBytes[i] == searchBytes[0])
            {
                if (searchBytes.Length == 1) { return i; }
                bool flag = true;
                for (int j = 1; j < searchBytes.Length; j++)
                {
                    if (srcBytes[i + j] != searchBytes[j])
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) { return i; }
            }
        }
        return -1;
    }

    /// <summary>
    /// 将流的位置对齐到指定值
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    public static void AlignUp(this Stream stream, int value)
    {
        long pos = stream.Position;
        pos = (pos + value - 1) & (~(value - 1));
        stream.Position = pos;
    }

    /// <summary>
    /// 将流的位置对齐到指定值
    /// </summary>
    /// <param name="brs"></param>
    /// <param name="value"></param>
    public static void AlignUp(this BinaryReader brs, int value)
    {
        var ms = brs.BaseStream;
        long pos = ms.Position;
        pos = (pos + value - 1) & (~(value - 1));
        ms.Position = pos;
    }

    public static void AlignUp(this BinaryWriter bw, int value)
    {
        var ms = bw.BaseStream as MemoryStream;
        Debug.Assert(ms != null, "此方法仅支持MemoryStream");

        long pos = ms.Position;
        pos = (pos + value - 1) & (~(value - 1));
        ms.Capacity = (int)Math.Max(ms.Capacity, pos + 1);
        ms.Position = pos;
    }

    /// <summary>
    /// 流跳过指定数量
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="size"></param>
    public static void Skip(this Stream stream, int size)
    {
        long pos = stream.Position;
        stream.Position = pos + size;
    }

    /// <summary>
    /// 流跳过指定数量
    /// </summary>
    /// <param name="br"></param>
    /// <param name="size"></param>
    public static void Skip(this BinaryReader br, int size)
    {
        var ms = br.BaseStream;
        long pos = ms.Position;
        ms.Position = pos + size;
    }
}