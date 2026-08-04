using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan;

public interface IGameSettingsSyntaxFactory
{
    SyntaxToken Create(string text, int rawKind, SyntaxTokenTrivia? leadingTrivia = null, SyntaxTokenTrivia? trailingTrivia = null);

    SyntaxToken Token(SyntaxTokenKind kind);
        
    SyntaxToken Identifier(string text);
}