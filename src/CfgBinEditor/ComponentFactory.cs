using CfgBinEditor.Components;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor;

internal class ComponentFactory(IEventBroker eventBroker) : IComponentFactory
{
    public RdbnValueComponent CreateRdbnValue(RdbnForm parentForm, object[] values, RdbnFieldDeclaration fieldDeclaration)
    {
        return new RdbnValueComponent(parentForm, values, fieldDeclaration, eventBroker);
    }
}
