using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5.Cryptography;
using Logic.Domain.Level5.Cryptography.InternalContract;

namespace Logic.Domain.Level5
{
    public class Level5Activator : IComponentActivator
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
            kernel.Register<IConfigurationReader, ConfigurationReader>(ActivationScope.Unique);
            kernel.Register<IConfigurationWriter, ConfigurationWriter>(ActivationScope.Unique);

            kernel.Register<IChecksumFactory, ChecksumFactory>(ActivationScope.Unique);

            kernel.RegisterConfiguration<Level5Configuration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
