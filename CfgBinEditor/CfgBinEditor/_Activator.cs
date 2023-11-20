using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            kernel.RegisterToSelf<MainForm>();
            kernel.RegisterToSelf<ConfigurationForm>();

            kernel.Register<IValueSettings, ValueSettings>(ActivationScope.Unique);
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
