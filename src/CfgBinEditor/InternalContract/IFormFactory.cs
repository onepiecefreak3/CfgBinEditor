using CfgBinEditor.Forms;
using Konnect.Contract.Management.Plugin;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.InternalContract;

public interface IFormFactory
{
    MainForm CreateMainForm();
    T2bForm CreateT2bForm(T2b config, IPluginManager pluginManager);
    RdbnForm CreateRdbnForm(Rdbn config, IPluginManager pluginManager);
    T2bTreeViewForm CreateT2bTreeViewForm(T2b config);
    RdbnTreeViewForm CreateRdbnTreeViewForm(Rdbn config);
}