using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public unsafe class LZ4Decoder : IDisposable
{
    public const int LZ4_MAX_INPUT_SIZE = 0x7E000000;   /* 2 113 929 216 bytes */

    private void* _context;
    private bool _disposed;

    public LZ4Decoder()
    {
        _context = LZ4Wrapper.LZ4_createStreamDecode();
        LZ4Wrapper.LZ4_setStreamDecode(_context, null, 0);
    }

    public static int LZ4_COMPRESSBOUND(int isize)
    {
        return isize > LZ4_MAX_INPUT_SIZE ? 0 : (isize) + ((isize) / 255) + 16;
    }

    public int LZ4_decompress_safe_continue(byte* source, byte* dest, int compressedSize, int maxOutputSize)
    {
        return LZ4Wrapper.LZ4_decompress_safe_continue(_context, source, dest, compressedSize, maxOutputSize);
    }

    ~LZ4Decoder()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {

        }

        LZ4Wrapper.LZ4_freeStreamDecode(_context);

        _disposed = true;
    }
}

public unsafe class LZ4Wrapper
{
    


    private const string LZ4_DLL = "LZ4.dll";

    

    [DllImport(LZ4_DLL)]
    public static extern void* LZ4_createStreamDecode();

    [DllImport(LZ4_DLL)]
    public static extern int LZ4_freeStreamDecode(void * LZ4_stream);

    [DllImport(LZ4_DLL)]
    public static extern int LZ4_setStreamDecode(void * LZ4_streamDecode, byte * dictionary, int dictSize);

    [DllImport(LZ4_DLL)]
    public static extern int LZ4_decompress_safe_continue(void * LZ4_streamDecode, byte * source, byte * dest, int compressedSize, int maxOutputSize);
}
