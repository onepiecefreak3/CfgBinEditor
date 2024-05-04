using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract.Exceptions;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.InternalContract
{
    [MapException(typeof(FormFactoryException))]
    public interface IFormFactory
    {
        MainForm CreateMainForm();
        T2bForm CreateT2bForm(T2b config);
        RdbnForm CreateRdbnForm(Rdbn config);
        T2bTreeViewForm CreateT2bTreeViewForm(T2b config, string gameName);
        RdbnTreeViewForm CreateRdbnTreeViewForm(Rdbn config);
    }
}
