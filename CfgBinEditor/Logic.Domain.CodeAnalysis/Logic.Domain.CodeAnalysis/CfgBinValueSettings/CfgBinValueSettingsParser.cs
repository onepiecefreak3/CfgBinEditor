using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsParser : ICfgBinValueSettingsParser
    {
        private readonly ITokenFactory<CfgBinValueSettingsSyntaxToken> _scriptFactory;
        private readonly ICfgBinValueSettingsSyntaxFactory _syntaxFactory;

        public CfgBinValueSettingsParser(ITokenFactory<CfgBinValueSettingsSyntaxToken> scriptFactory, ICfgBinValueSettingsSyntaxFactory syntaxFactory)
        {
            _scriptFactory = scriptFactory;
            _syntaxFactory = syntaxFactory;
        }

        public ConfigUnitSyntax Parse(string text)
        {
            IBuffer<CfgBinValueSettingsSyntaxToken> buffer = CreateTokenBuffer(text);

            return ParseConfigUnit(buffer);
        }

        private ConfigUnitSyntax ParseConfigUnit(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            var gameConfigs = new List<GameConfigSyntax>();

            while (!buffer.IsEndOfInput)
                gameConfigs.Add(ParseGameConfig(buffer));

            return new ConfigUnitSyntax(gameConfigs);
        }

        private GameConfigSyntax ParseGameConfig(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            SyntaxToken identifier = ParseIdentifierToken(buffer);
            SyntaxToken bracketOpen = ParseBracketOpenToken(buffer);

            var entries = new List<EntryConfigSyntax>();
            while (!HasTokenKind(buffer, SyntaxTokenKind.BracketClose))
                entries.Add(ParseEntryConfigSyntax(buffer));

            SyntaxToken bracketClose = ParseBracketCloseToken(buffer);

            return new GameConfigSyntax(identifier, bracketOpen, entries, bracketClose);
        }

        private EntryConfigSyntax ParseEntryConfigSyntax(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            SyntaxToken identifier = ParseIdentifierToken(buffer);
            SyntaxToken parenOpen = ParseParenOpenToken(buffer);

            var entries = new List<EntryConfigSettingSyntax>();
            while (!HasTokenKind(buffer, SyntaxTokenKind.ParenClose))
                entries.Add(ParseEntryConfigSettingSyntax(buffer));

            SyntaxToken parenClose = ParseParenCloseToken(buffer);

            return new EntryConfigSyntax(identifier, parenOpen, entries, parenClose);
        }

        private EntryConfigSettingSyntax ParseEntryConfigSettingSyntax(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            SyntaxToken name = ParseIdentifierToken(buffer);
            SyntaxToken pipe = ParsePipeToken(buffer);

            SyntaxToken isHex;
            if (HasTokenKind(buffer, SyntaxTokenKind.TrueKeyword))
                isHex = ParseTrueKeywordToken(buffer);
            else if (HasTokenKind(buffer, SyntaxTokenKind.FalseKeyword))
                isHex = ParseFalseKeywordToken(buffer);
            else
                throw CreateException(buffer, "Unknown value for hexadecimal setting.", SyntaxTokenKind.TrueKeyword,
                    SyntaxTokenKind.FalseKeyword);

            return new EntryConfigSettingSyntax(name, pipe, isHex);
        }

        private SyntaxToken ParseIdentifierToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.Identifier);
        }

        private SyntaxToken ParseBracketOpenToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.BracketOpen);
        }

        private SyntaxToken ParseBracketCloseToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.BracketClose);
        }

        private SyntaxToken ParseParenOpenToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.ParenOpen);
        }

        private SyntaxToken ParseParenCloseToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.ParenClose);
        }

        private SyntaxToken ParseTrueKeywordToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.TrueKeyword);
        }

        private SyntaxToken ParseFalseKeywordToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.FalseKeyword);
        }

        private SyntaxToken ParsePipeToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.Pipe);
        }

        private SyntaxToken CreateToken(IBuffer<CfgBinValueSettingsSyntaxToken> buffer, SyntaxTokenKind expectedKind)
        {
            SyntaxTokenTrivia? leadingTrivia = ReadTrivia(buffer);

            if (buffer.Peek().Kind != expectedKind)
                throw CreateException(buffer, $"Unexpected token {buffer.Peek().Kind}.", expectedKind);
            CfgBinValueSettingsSyntaxToken content = buffer.Read();

            SyntaxTokenTrivia? trailingTrivia = ReadTrivia(buffer);

            return _syntaxFactory.Create(content.Text, (int)expectedKind, leadingTrivia, trailingTrivia);
        }

        private SyntaxTokenTrivia? ReadTrivia(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
            {
                CfgBinValueSettingsSyntaxToken token = buffer.Read();
                return new SyntaxTokenTrivia(token.Text);
            }

            return null;
        }

        protected bool HasTokenKind(IBuffer<CfgBinValueSettingsSyntaxToken> buffer, SyntaxTokenKind expectedKind)
        {
            return HasTokenKind(buffer, 0, expectedKind);
        }

        protected bool HasTokenKind(IBuffer<CfgBinValueSettingsSyntaxToken> buffer, int position, SyntaxTokenKind expectedKind)
        {
            var toPeek = 0;
            CfgBinValueSettingsSyntaxToken peekedToken = buffer.Peek(toPeek);

            position = Math.Max(0, position);
            for (var i = 0; i < position + 1; i++)
            {
                peekedToken = buffer.Peek(toPeek++);
                if (peekedToken.Kind == SyntaxTokenKind.Trivia)
                    peekedToken = buffer.Peek(toPeek++);
            }

            return peekedToken.Kind == expectedKind;
        }

        private IBuffer<CfgBinValueSettingsSyntaxToken> CreateTokenBuffer(string text)
        {
            ILexer<CfgBinValueSettingsSyntaxToken> lexer = _scriptFactory.CreateLexer(text);
            return _scriptFactory.CreateTokenBuffer(lexer);
        }

        private (int, int) GetCurrentLineAndColumn(IBuffer<CfgBinValueSettingsSyntaxToken> buffer)
        {
            var toPeek = 0;

            if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
                toPeek++;

            CfgBinValueSettingsSyntaxToken token = buffer.Peek(toPeek);
            return (token.Line, token.Column);
        }

        private Exception CreateException(IBuffer<CfgBinValueSettingsSyntaxToken> buffer, string message, params SyntaxTokenKind[] expected)
        {
            (int line, int column) = GetCurrentLineAndColumn(buffer);
            return CreateException(message, line, column, expected);
        }

        private Exception CreateException(string message, int line, int column, params SyntaxTokenKind[] expected)
        {
            message = $"{message} (Line {line}, Column {column})";

            if (expected.Length > 0)
            {
                message = expected.Length == 1 ?
                    $"{message} (Expected {expected[0]})" :
                    $"{message} (Expected any of {string.Join(", ", expected)})";
            }

            throw new InvalidOperationException(message);
        }
    }
}
