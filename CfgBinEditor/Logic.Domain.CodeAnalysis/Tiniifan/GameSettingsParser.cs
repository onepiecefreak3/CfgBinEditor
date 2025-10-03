using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsParser : IGameSettingsParser
    {
        private readonly ITokenFactory<GameSettingsSyntaxToken> _scriptFactory;
        private readonly IGameSettingsSyntaxFactory _syntaxFactory;

        public GameSettingsParser(ITokenFactory<GameSettingsSyntaxToken> scriptFactory, IGameSettingsSyntaxFactory syntaxFactory)
        {
            _scriptFactory = scriptFactory;
            _syntaxFactory = syntaxFactory;
        }

        public ConfigUnitSyntax Parse(string text)
        {
            IBuffer<GameSettingsSyntaxToken> buffer = CreateTokenBuffer(text);

            return ParseConfigUnit(buffer);
        }

        private ConfigUnitSyntax ParseConfigUnit(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            var gameConfigs = new List<GameConfigSyntax>();

            while (!buffer.IsEndOfInput)
                gameConfigs.Add(ParseGameConfig(buffer));

            return new ConfigUnitSyntax(gameConfigs);
        }

        private GameConfigSyntax ParseGameConfig(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            SyntaxToken[] name = ParseCompositeValueToken(buffer, SyntaxTokenKind.BracketOpen).ToArray();
            SyntaxToken bracketOpen = ParseBracketOpenToken(buffer);

            var entries = new List<EntryConfigSyntax>();
            while (!HasTokenKind(buffer, SyntaxTokenKind.BracketClose))
                entries.Add(ParseEntryConfigSyntax(buffer));

            SyntaxToken bracketClose = ParseBracketCloseToken(buffer);

            return new GameConfigSyntax(name, bracketOpen, entries, bracketClose);
        }

        private EntryConfigSyntax ParseEntryConfigSyntax(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            SyntaxToken[] name = ParseCompositeValueToken(buffer, SyntaxTokenKind.ParenOpen).ToArray();
            SyntaxToken parenOpen = ParseParenOpenToken(buffer);

            var entries = new List<EntryConfigSettingSyntax>();
            while (!HasTokenKind(buffer, SyntaxTokenKind.ParenClose))
                entries.Add(ParseEntryConfigSettingSyntax(buffer));

            SyntaxToken parenClose = ParseParenCloseToken(buffer);

            return new EntryConfigSyntax(name, parenOpen, entries, parenClose);
        }

        private EntryConfigSettingSyntax ParseEntryConfigSettingSyntax(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            SyntaxToken[] value1 = ParseCompositeValueToken(buffer, SyntaxTokenKind.Pipe).ToArray();
            SyntaxToken pipe = ParsePipeToken(buffer);
            SyntaxToken[] value2 = ParseEndCompositeValueToken(buffer).ToArray();

            return new EntryConfigSettingSyntax(value1, pipe, value2);
        }

        private IEnumerable<SyntaxToken> ParseCompositeValueToken(IBuffer<GameSettingsSyntaxToken> buffer, SyntaxTokenKind endKind)
        {
            while (buffer.Peek().Kind != endKind)
                yield return CreateToken(buffer, buffer.Peek().Kind);
        }

        private IEnumerable<SyntaxToken> ParseEndCompositeValueToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            SyntaxToken valueToken;
            do
            {
                valueToken = CreateToken(buffer, buffer.Peek().Kind);
                yield return valueToken;
            } while (!valueToken.TrailingTrivia?.Text.Contains('\n') ?? true);
        }

        private SyntaxToken ParseBracketOpenToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.BracketOpen);
        }

        private SyntaxToken ParseBracketCloseToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.BracketClose);
        }

        private SyntaxToken ParseParenOpenToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.ParenOpen);
        }

        private SyntaxToken ParseParenCloseToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.ParenClose);
        }

        private SyntaxToken ParsePipeToken(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            return CreateToken(buffer, SyntaxTokenKind.Pipe);
        }

        private SyntaxToken CreateToken(IBuffer<GameSettingsSyntaxToken> buffer, SyntaxTokenKind expectedKind)
        {
            SyntaxTokenTrivia? leadingTrivia = ReadTrivia(buffer);

            if (buffer.Peek().Kind != expectedKind)
                throw CreateException(buffer, $"Unexpected token {buffer.Peek().Kind}.", expectedKind);
            GameSettingsSyntaxToken content = buffer.Read();

            SyntaxTokenTrivia? trailingTrivia = ReadTrivia(buffer);

            return _syntaxFactory.Create(content.Text, (int)expectedKind, leadingTrivia, trailingTrivia);
        }

        private SyntaxTokenTrivia? ReadTrivia(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
            {
                GameSettingsSyntaxToken token = buffer.Read();
                return new SyntaxTokenTrivia(token.Text);
            }

            return null;
        }

        protected bool HasTokenKind(IBuffer<GameSettingsSyntaxToken> buffer, SyntaxTokenKind expectedKind)
        {
            return HasTokenKind(buffer, 0, expectedKind);
        }

        protected bool HasTokenKind(IBuffer<GameSettingsSyntaxToken> buffer, int position, SyntaxTokenKind expectedKind)
        {
            var toPeek = 0;
            GameSettingsSyntaxToken peekedToken = buffer.Peek(toPeek);

            position = Math.Max(0, position);
            for (var i = 0; i < position + 1; i++)
            {
                peekedToken = buffer.Peek(toPeek++);
                if (peekedToken.Kind == SyntaxTokenKind.Trivia)
                    peekedToken = buffer.Peek(toPeek++);
            }

            return peekedToken.Kind == expectedKind;
        }

        private IBuffer<GameSettingsSyntaxToken> CreateTokenBuffer(string text)
        {
            ILexer<GameSettingsSyntaxToken> lexer = _scriptFactory.CreateLexer(text);
            return _scriptFactory.CreateTokenBuffer(lexer);
        }

        private (int, int) GetCurrentLineAndColumn(IBuffer<GameSettingsSyntaxToken> buffer)
        {
            var toPeek = 0;

            if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
                toPeek++;

            GameSettingsSyntaxToken token = buffer.Peek(toPeek);
            return (token.Line, token.Column);
        }

        private Exception CreateException(IBuffer<GameSettingsSyntaxToken> buffer, string message, params SyntaxTokenKind[] expected)
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
