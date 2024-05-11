using CfgBinEditor.Components;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor
{
    internal class ComponentFactory : IComponentFactory
    {
        private readonly ICoCoKernel _kernel;

        public ComponentFactory(ICoCoKernel kernel)
        {
            _kernel = kernel;
        }

        public RdbnValueComponent CreateRdbnValue(RdbnForm parentForm, object[] values, RdbnFieldDeclaration fieldDeclaration)
        {
            return _kernel.Get<RdbnValueComponent>(
                new ConstructorParameter("parentForm", parentForm),
                new ConstructorParameter("values", values),
                new ConstructorParameter("fieldDeclaration", fieldDeclaration));
        }
    }
}
