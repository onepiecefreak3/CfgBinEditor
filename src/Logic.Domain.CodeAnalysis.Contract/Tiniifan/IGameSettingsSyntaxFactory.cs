using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan
{
    [MapException(typeof(GameSettingsSyntaxFactoryException))]
    public interface IGameSettingsSyntaxFactory
    {
        SyntaxToken Create(string text, int rawKind, SyntaxTokenTrivia? leadingTrivia = null, SyntaxTokenTrivia? trailingTrivia = null);

        SyntaxToken Token(SyntaxTokenKind kind);
        
        SyntaxToken Identifier(string text);
    }
}
