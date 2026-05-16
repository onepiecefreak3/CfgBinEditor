using Logic.Domain.CodeAnalysis.Contract;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsFactory: ITokenFactory<GameSettingsSyntaxToken>
    {
        private readonly ICoCoKernel _kernel;

        public GameSettingsFactory(ICoCoKernel kernel)
        {
            _kernel = kernel;
        }

        public ILexer<GameSettingsSyntaxToken> CreateLexer(string text)
        {
            var buffer = _kernel.Get<IBuffer<int>>(
                new ConstructorParameter("text", text));
            return _kernel.Get<ILexer<GameSettingsSyntaxToken>>(
                new ConstructorParameter("buffer", buffer));
        }

        public IBuffer<GameSettingsSyntaxToken> CreateTokenBuffer(ILexer<GameSettingsSyntaxToken> lexer)
        {
            return _kernel.Get<IBuffer<GameSettingsSyntaxToken>>(new ConstructorParameter("lexer", lexer));
        }
    }
}
