using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Business.CfgBinEditorManagement.InternalContract;

namespace Logic.Business.CfgBinEditorManagement
{
    public class CfgBinEditorManagementActivator : IComponentActivator
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
            kernel.Register<IEntryNamesProvider, EntryNamesProvider>(ActivationScope.Unique);

            kernel.Register<IGameSettingsReader<ValueSettingEntry>, ValueSettingsReader>(ActivationScope.Unique);
            kernel.Register<IValueSettingsWriter, ValueSettingsWriter>(ActivationScope.Unique);

            kernel.Register<IGameSettingsReader<EntryNameEntry>, EntryNamesReader>(ActivationScope.Unique);

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
