using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsSyntaxFactory : ICfgBinValueSettingsSyntaxFactory
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
                default: throw new InvalidOperationException($"Cannot create simple token from kind {kind}. Use other methods instead.");
            }
        }

        public SyntaxToken Identifier(string text)
        {
            return new SyntaxToken(text, (int)SyntaxTokenKind.Identifier);
        }
    }
}
