using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.Tiniifan;

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
            kernel.Register<ITokenFactory<GameSettingsSyntaxToken>, GameSettingsFactory>(ActivationScope.Unique);
            kernel.Register<ILexer<GameSettingsSyntaxToken>, GameSettingsLexer>();
            kernel.Register<IBuffer<GameSettingsSyntaxToken>, TokenBuffer<GameSettingsSyntaxToken>>();
            kernel.Register<IBuffer<int>, StringBuffer>();

            kernel.Register<IGameSettingsParser, GameSettingsParser>(ActivationScope.Unique);
            kernel.Register<IGameSettingsComposer, GameSettingsComposer>(ActivationScope.Unique);
            kernel.Register<IGameSettingsWhitespaceNormalizer, GameSettingsWhitespaceNormalizer>(ActivationScope.Unique);

            kernel.Register<IGameSettingsSyntaxFactory, GameSettingsSyntaxFactory>();

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
