using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings;

namespace Logic.Domain.CodeAnalysis
{
    public class CodeAnalysisActivator : IComponentActivator
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
            kernel.Register<ITokenFactory<CfgBinValueSettingsSyntaxToken>, CfgBinValueSettingsFactory>(ActivationScope.Unique);
            kernel.Register<ILexer<CfgBinValueSettingsSyntaxToken>, CfgBinValueSettingsLexer>();
            kernel.Register<IBuffer<CfgBinValueSettingsSyntaxToken>, TokenBuffer<CfgBinValueSettingsSyntaxToken>>();
            kernel.Register<IBuffer<int>, StringBuffer>();

            kernel.Register<ICfgBinValueSettingsParser, CfgBinValueSettingsParser>(ActivationScope.Unique);
            kernel.Register<ICfgBinValueSettingsComposer, CfgBinValueSettingsComposer>(ActivationScope.Unique);
            kernel.Register<ICfgBinValueSettingsWhitespaceNormalizer, CfgBinValueSettingsWhitespaceNormalizer>(ActivationScope.Unique);

            kernel.Register<ICfgBinValueSettingsSyntaxFactory, CfgBinValueSettingsSyntaxFactory>();

            kernel.RegisterConfiguration<CodeAnalysisConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
