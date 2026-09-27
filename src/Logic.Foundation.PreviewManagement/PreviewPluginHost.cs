using System.Security.Cryptography;
using System.Text;
using Konnect.Contract.Management.Plugin;
using Konnect.Management.Plugin;
using Konnect.Management.Plugin.Loaders;
using Logic.Foundation.PreviewManagement.Abstract;

namespace Logic.Foundation.PreviewManagement;

public static class PreviewPluginHost
{
    public static IPluginManager Create(string pluginDirectory)
    {
        string managedDirectory = PrepareManagedPluginDirectory(pluginDirectory);
        var managed = new PluginManager(new PluginLoader<IPreviewPlugin>(managedDirectory));
        IReadOnlyList<IPreviewPlugin> native = NativePreviewPluginLoader.Load(pluginDirectory);
        return new CompositePluginManager(managed, native);
    }

    private static string PrepareManagedPluginDirectory(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
            return pluginDirectory;

        string[] libraries = Directory.GetFiles(pluginDirectory, "*.dll");
        if (libraries.All(IsManagedAssembly))
            return pluginDirectory;

        string shadow = Path.Combine(Path.GetTempPath(), "CfgBinEditor", "managed-plugins", DirectoryKey(pluginDirectory));
        Directory.CreateDirectory(shadow);
        foreach (string existing in Directory.GetFiles(shadow, "*.dll"))
            File.Delete(existing);

        foreach (string library in libraries.Where(IsManagedAssembly))
            File.Copy(library, Path.Combine(shadow, Path.GetFileName(library)), overwrite: true);

        return shadow;
    }

    private static string DirectoryKey(string pluginDirectory)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(pluginDirectory)));
        return Convert.ToHexString(hash)[..16];
    }

    private static bool IsManagedAssembly(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            if (stream.Length < 0x40 || reader.ReadUInt16() != 0x5A4D)
                return false;

            stream.Position = 0x3C;
            int peOffset = reader.ReadInt32();
            if (peOffset <= 0 || stream.Length < peOffset + 24)
                return false;

            stream.Position = peOffset;
            if (reader.ReadUInt32() != 0x4550)
                return false;

            stream.Position += 20;
            ushort magic = reader.ReadUInt16();
            bool pe32Plus = magic == 0x20B;
            if (magic != 0x10B && !pe32Plus)
                return false;

            int dataDirectoryOffset = peOffset + 24 + (pe32Plus ? 112 : 96);
            if (stream.Length < dataDirectoryOffset)
                return false;

            stream.Position = dataDirectoryOffset - 4;
            if (reader.ReadUInt32() <= 14)
                return false;

            int cliOffset = dataDirectoryOffset + 14 * 8;
            if (stream.Length < cliOffset + 8)
                return false;

            stream.Position = cliOffset;
            uint rva = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            return rva != 0 && size != 0;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
