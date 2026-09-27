using Konnect.Contract.DataClasses.Management.Plugin.Loaders;
using Konnect.Contract.Management.Plugin;
using Konnect.Contract.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;

namespace Logic.Foundation.PreviewManagement;

internal sealed class CompositePluginManager(IPluginManager managed, IReadOnlyList<IPreviewPlugin> native) : IPluginManager
{
    public IReadOnlyList<PluginLoadError> GetErrors()
    {
        return managed.GetErrors();
    }

    public IEnumerable<TPlugin> GetPlugins<TPlugin>()
        where TPlugin : IPlugin
    {
        var yielded = new HashSet<Guid>();
        foreach (IPreviewPlugin plugin in native)
        {
            if (plugin is not TPlugin typed)
                continue;

            yielded.Add(plugin.PluginId);
            yield return typed;
        }

        foreach (TPlugin plugin in managed.GetPlugins<TPlugin>())
        {
            if (plugin is IPlugin identified && yielded.Contains(identified.PluginId))
                continue;

            yield return plugin;
        }
    }

    public TPlugin GetPlugin<TPlugin>(Guid pluginId)
        where TPlugin : IPlugin
    {
        foreach (IPreviewPlugin plugin in native)
        {
            if (plugin.PluginId == pluginId && plugin is TPlugin typed)
                return typed;
        }

        return managed.GetPlugin<TPlugin>(pluginId)!;
    }
}
