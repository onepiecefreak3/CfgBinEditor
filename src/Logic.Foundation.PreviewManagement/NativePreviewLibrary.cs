using System.Runtime.InteropServices;
using System.Text;

namespace Logic.Foundation.PreviewManagement;

internal sealed unsafe class NativePreviewLibrary
{
    private readonly IntPtr _handle;
    private readonly delegate* unmanaged[Cdecl]<int> _getCount;
    private readonly delegate* unmanaged[Cdecl]<int, Guid*, int> _getId;
    private readonly delegate* unmanaged[Cdecl]<int, ByteBuffer*, int> _getMetadata;
    private readonly delegate* unmanaged[Cdecl]<int, byte*, int, ByteBuffer*, int> _deserialize;
    private readonly delegate* unmanaged[Cdecl]<int, byte*, int, ByteBuffer*, int> _render;
    private readonly delegate* unmanaged[Cdecl]<ByteBuffer*, int> _getLastError;
    private readonly delegate* unmanaged[Cdecl]<void*, void> _free;

    private NativePreviewLibrary(IntPtr handle)
    {
        _handle = handle;
        _getCount = (delegate* unmanaged[Cdecl]<int>)Export(handle, "cbe_plugin_get_count");
        _getId = (delegate* unmanaged[Cdecl]<int, Guid*, int>)Export(handle, "cbe_plugin_get_id");
        _getMetadata = (delegate* unmanaged[Cdecl]<int, ByteBuffer*, int>)Export(handle, "cbe_plugin_get_metadata");
        _deserialize = (delegate* unmanaged[Cdecl]<int, byte*, int, ByteBuffer*, int>)Export(handle, "cbe_plugin_deserialize");
        _render = (delegate* unmanaged[Cdecl]<int, byte*, int, ByteBuffer*, int>)Export(handle, "cbe_plugin_render");
        _getLastError = (delegate* unmanaged[Cdecl]<ByteBuffer*, int>)Export(handle, "cbe_plugin_get_last_error");
        _free = (delegate* unmanaged[Cdecl]<void*, void>)Export(handle, "cbe_plugin_free");
    }

    public static bool TryLoad(string path, out NativePreviewLibrary? library)
    {
        library = null;
        if (!NativeLibrary.TryLoad(path, out IntPtr handle))
            return false;

        if (!NativeLibrary.TryGetExport(handle, "cbe_plugin_get_count", out _))
        {
            NativeLibrary.Free(handle);
            return false;
        }

        library = new NativePreviewLibrary(handle);
        return true;
    }

    public int Count => _getCount();

    public Guid GetId(int index)
    {
        Guid pluginId;
        EnsureSuccess(_getId(index, &pluginId));
        return pluginId;
    }

    public byte[] GetMetadata(int index)
    {
        ByteBuffer buffer;
        EnsureSuccess(_getMetadata(index, &buffer));
        return Take(buffer);
    }

    public byte[] Deserialize(int index, string text)
    {
        byte[] encoded = Encoding.UTF8.GetBytes(text);
        fixed (byte* textPointer = encoded)
        {
            ByteBuffer buffer;
            byte* pointer = encoded.Length == 0 ? null : textPointer;
            EnsureSuccess(_deserialize(index, pointer, encoded.Length, &buffer));
            return Take(buffer);
        }
    }

    public byte[] Render(int index, byte[] wire)
    {
        fixed (byte* wirePointer = wire)
        {
            ByteBuffer buffer;
            byte* pointer = wire.Length == 0 ? null : wirePointer;
            EnsureSuccess(_render(index, pointer, wire.Length, &buffer));
            return Take(buffer);
        }
    }

    private byte[] Take(ByteBuffer buffer)
    {
        try
        {
            if (buffer.Data == 0 || buffer.Length <= 0)
                return [];

            byte[] copy = new byte[buffer.Length];
            Marshal.Copy(buffer.Data, copy, 0, buffer.Length);
            return copy;
        }
        finally
        {
            if (buffer.Data != 0)
                _free((void*)buffer.Data);
        }
    }

    private void EnsureSuccess(int status)
    {
        if (status == 0)
            return;

        ByteBuffer error;
        if (_getLastError(&error) == 0)
        {
            byte[] message = Take(error);
            throw new InvalidOperationException(Encoding.UTF8.GetString(message));
        }

        throw new InvalidOperationException("Native preview plugin call failed with status " + status + ".");
    }

    private static IntPtr Export(IntPtr handle, string name)
    {
        if (!NativeLibrary.TryGetExport(handle, name, out IntPtr export))
            throw new InvalidOperationException("Native preview plugin is missing export " + name + ".");

        return export;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByteBuffer
    {
        public IntPtr Data;
        public int Length;
    }
}
