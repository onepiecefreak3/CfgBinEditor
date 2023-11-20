using Logic.Domain.CodeAnalysis.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings.InternalContract.DataClasses;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsFactory: ITokenFactory<CfgBinValueSettingsSyntaxToken>
    {
        private readonly ICoCoKernel _kernel;

        public CfgBinValueSettingsFactory(ICoCoKernel kernel)
        {
            _kernel = kernel;
        }

        public ILexer<CfgBinValueSettingsSyntaxToken> CreateLexer(string text)
        {
            var buffer = _kernel.Get<IBuffer<int>>(
                new ConstructorParameter("text", text));
            return _kernel.Get<ILexer<CfgBinValueSettingsSyntaxToken>>(
                new ConstructorParameter("buffer", buffer));
        }

        public IBuffer<CfgBinValueSettingsSyntaxToken> CreateTokenBuffer(ILexer<CfgBinValueSettingsSyntaxToken> lexer)
        {
            return _kernel.Get<IBuffer<CfgBinValueSettingsSyntaxToken>>(new ConstructorParameter("lexer", lexer));
        }
    }
}
