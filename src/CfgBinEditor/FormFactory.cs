using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Settings;
using Konnect.Contract.Management.Plugin;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor;

internal class FormFactory(
    ICoCoKernel kernel,
    IEventBroker eventBroker,
    IT2bWriter t2bWriter,
    IRdbnWriter rdbnWriter,
    IValueSettingsProvider valueSettingsProvider,
    ISettingsProvider settingsProvider,
    IEntryNamesProvider entryNamesProvider,
    IComponentFactory componentFactory) : IFormFactory
{
    public MainForm CreateMainForm()
    {
        return kernel.Get<MainForm>();
    }

    public T2bForm CreateT2bForm(T2b config, IPluginManager pluginManager)
    {
        return new T2bForm(config, pluginManager, this, eventBroker, t2bWriter, valueSettingsProvider);
    }

    public RdbnForm CreateRdbnForm(Rdbn config, IPluginManager pluginManager)
    {
        return new RdbnForm(config, this, pluginManager, componentFactory, eventBroker, rdbnWriter);
    }

    public T2bTreeViewForm CreateT2bTreeViewForm(T2b config)
    {
        return new T2bTreeViewForm(config, eventBroker, settingsProvider, valueSettingsProvider, entryNamesProvider);
    }

    public RdbnTreeViewForm CreateRdbnTreeViewForm(Rdbn config)
    {
        return new RdbnTreeViewForm(config, eventBroker);
    }
}
