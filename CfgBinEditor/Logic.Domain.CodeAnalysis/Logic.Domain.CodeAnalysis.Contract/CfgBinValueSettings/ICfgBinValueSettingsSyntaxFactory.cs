using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings
{
    [MapException(typeof(CfgBinValueSettingsSyntaxFactoryException))]
    public interface ICfgBinValueSettingsSyntaxFactory
    {
        SyntaxToken Create(string text, int rawKind, SyntaxTokenTrivia? leadingTrivia = null, SyntaxTokenTrivia? trailingTrivia = null);

        SyntaxToken Token(SyntaxTokenKind kind);
        
        SyntaxToken Identifier(string text);
    }
}
