using CfgBinEditor.Components;
using CfgBinEditor.Forms;
using CfgBinEditor.InternalContract;
using CfgBinEditor.resources;
using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Localization;

namespace CfgBinEditor
{
    public class CfgBinEditorActivator : IComponentActivator
    {
        public void Activating()
        {
        }

        public void Activated()
        {
        }

        public void Deactivating()
        {
        }

        public void Deactivated()
        {
        }

        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<IFormFactory, FormFactory>(ActivationScope.Unique);
            kernel.Register<IComponentFactory, ComponentFactory>(ActivationScope.Unique);

            kernel.RegisterToSelf<MainForm>();
            kernel.RegisterToSelf<T2bForm>();
            kernel.RegisterToSelf<RdbnForm>();

            kernel.RegisterToSelf<T2bTreeViewForm>();
            kernel.RegisterToSelf<RdbnTreeViewForm>();

            kernel.RegisterToSelf<RdbnValueComponent>();

            kernel.Register<ILocalizer, Localizer>(ActivationScope.Unique);

            kernel.RegisterConfiguration<CfgBinEditorConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
