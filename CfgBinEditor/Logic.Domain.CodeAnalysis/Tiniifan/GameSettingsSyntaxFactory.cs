using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsSyntaxFactory : IGameSettingsSyntaxFactory
    {
        public SyntaxToken Create(string text, int rawKind, SyntaxTokenTrivia? leadingTrivia = null,
            SyntaxTokenTrivia? trailingTrivia = null)
        {
            return new SyntaxToken(text, rawKind, leadingTrivia, trailingTrivia);
        }

        public SyntaxToken Token(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.BracketOpen: return new("[", (int)kind);
                case SyntaxTokenKind.BracketClose: return new("]", (int)kind);
                case SyntaxTokenKind.ParenOpen: return new("(", (int)kind);
                case SyntaxTokenKind.ParenClose: return new(")", (int)kind);
                case SyntaxTokenKind.Pipe: return new("|", (int)kind);
                case SyntaxTokenKind.TrueKeyword: return new("True", (int)kind);
                case SyntaxTokenKind.FalseKeyword: return new("False", (int)kind);
                default: throw new InvalidOperationException($"Cannot create simple token from kind {kind}. Use other methods instead.");
            }
        }

        public SyntaxToken Identifier(string text)
        {
            return new SyntaxToken(text, (int)SyntaxTokenKind.Identifier);
        }
    }
}
