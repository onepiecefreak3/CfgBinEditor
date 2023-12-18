using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract.Exceptions;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor.InternalContract
{
    [MapException(typeof(FormFactoryException))]
    public interface IFormFactory
    {
        MainForm CreateMainForm();
        ConfigurationForm CreateConfigurationForm(Configuration config);
        ConfigurationTreeViewForm CreateConfigurationTreeViewForm(Configuration config, string gameName);
    }
}
