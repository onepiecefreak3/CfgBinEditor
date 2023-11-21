using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.CfgBinValueSettingsManagement.Contract;

namespace Logic.Business.CfgBinValueSettingsManagement
{
    public class CfgBinValueSettingsManagementActivator : IComponentActivator
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
            kernel.Register<IValueSettingsProvider, ValueSettingsProvider>(ActivationScope.Unique);
            kernel.Register<IValueSettingsReader, ValueSettingsReader>(ActivationScope.Unique);
            kernel.Register<IValueSettingsWriter, ValueSettingsWriter>(ActivationScope.Unique);

            kernel.RegisterConfiguration<CfgBinValueSettingsManagementConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
