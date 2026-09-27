using Logic.Foundation.PreviewManagement.Abstract;

namespace Logic.Foundation.PreviewManagement;

internal static class NativePreviewPluginLoader
{
    private static readonly List<NativePreviewLibrary> Libraries = [];

    public static IReadOnlyList<IPreviewPlugin> Load(string directory)
    {
        if (!Directory.Exists(directory))
            return [];

        var plugins = new List<IPreviewPlugin>();
        foreach (string path in Directory.EnumerateFiles(directory, "*.dll"))
        {
            if (!NativePreviewLibrary.TryLoad(path, out NativePreviewLibrary? library) || library is null)
                continue;

            Libraries.Add(library);
            int count = library.Count;
            for (int index = 0; index < count; index++)
                plugins.Add(new NativePreviewPlugin(library, index));
        }

        return plugins;
    }
}
