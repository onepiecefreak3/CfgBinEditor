using CfgBinEditor.Components;
using Logic.Domain.Level5Management.Contract.DataClasses;
using CfgBinEditor.Forms;

namespace CfgBinEditor.InternalContract;

public interface IComponentFactory
{
    RdbnValueComponent CreateRdbnValue(RdbnForm parentForm, object[] values, RdbnFieldDeclaration fieldDeclaration);
}