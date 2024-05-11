using Logic.Domain.CodeAnalysis.Contract;
using System.Text;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsLexer : ILexer<GameSettingsSyntaxToken>
    {
        private readonly StringBuilder _sb;
        private readonly IBuffer<int> _buffer;

        public bool IsEndOfInput => _buffer.IsEndOfInput;

        private int Line { get; set; } = 1;
        private int Column { get; set; } = 1;
        private int Position { get; set; }

        public GameSettingsLexer(IBuffer<int> buffer)
        {
            _sb = new StringBuilder();
            _buffer = buffer;
        }

        public GameSettingsSyntaxToken Read()
        {
            if (!TryPeekChar(out char character))
                return new GameSettingsSyntaxToken(SyntaxTokenKind.EndOfFile, Position, Line, Column);

            switch (character)
            {
                case '[':
                    return new GameSettingsSyntaxToken(SyntaxTokenKind.BracketOpen, Position, Line, Column, $"{ReadChar()}");
                case ']':
                    return new GameSettingsSyntaxToken(SyntaxTokenKind.BracketClose, Position, Line, Column, $"{ReadChar()}");
                case '(':
                    return new GameSettingsSyntaxToken(SyntaxTokenKind.ParenOpen, Position, Line, Column, $"{ReadChar()}");
                case ')':
                    return new GameSettingsSyntaxToken(SyntaxTokenKind.ParenClose, Position, Line, Column, $"{ReadChar()}");
                case '|':
                    return new GameSettingsSyntaxToken(SyntaxTokenKind.Pipe, Position, Line, Column, $"{ReadChar()}");

                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return ReadTriviaAndComments();

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumericLiteral();

                default:
                    return ReadIdentifierOrKeyword();
            }
        }

        private GameSettingsSyntaxToken ReadIdentifierOrKeyword()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case '[':
                    case ']':
                    case '(':
                    case ')':
                    case '|':
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;

                    default:
                        _sb.Append(ReadChar());
                        continue;
                }

                break;
            }

            var finalValue = _sb.ToString();

            if (finalValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                return new GameSettingsSyntaxToken(SyntaxTokenKind.TrueKeyword, position, line, column, finalValue);

            if (finalValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                return new GameSettingsSyntaxToken(SyntaxTokenKind.FalseKeyword, position, line, column, finalValue);

            return new GameSettingsSyntaxToken(SyntaxTokenKind.Identifier, position, line, column, finalValue);
        }

        private GameSettingsSyntaxToken ReadTriviaAndComments()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case '/':
                        if (!IsPeekedChar(1, '/'))
                            break;

                        _sb.Append(ReadChar());
                        _sb.Append(ReadChar());

                        while (!IsPeekedChar('\n'))
                            _sb.Append(ReadChar());

                        continue;

                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        _sb.Append(ReadChar());
                        continue;
                }

                break;
            }

            return new GameSettingsSyntaxToken(SyntaxTokenKind.Trivia, position, line, column, _sb.ToString());
        }

        private GameSettingsSyntaxToken ReadNumericLiteral()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            var isHex = false;
            var kind = SyntaxTokenKind.NumericLiteral;

            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case '0':
                        if (!IsPeekedChar(1, 'x'))
                            goto case '1';

                        if (_sb.Length != 0)
                            throw CreateException($"Invalid hex identifier in numeric literal {character} in numeric literal.");

                        _sb.Append(ReadChar());
                        _sb.Append(ReadChar());

                        isHex = true;
                        continue;

                    case '-':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        _sb.Append(ReadChar());
                        continue;

                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                        if (!isHex)
                            throw CreateException("Invalid character in numeric literal.");

                        _sb.Append(ReadChar());
                        continue;
                }

                break;
            }

            return new GameSettingsSyntaxToken(kind, position, line, column, _sb.ToString());
        }

        private bool IsPeekedChar(char expected)
        {
            return IsPeekedChar(0, expected);
        }

        private bool IsPeekedChar(int position, char expected)
        {
            return TryPeekChar(position, out char character) && character == expected;
        }

        private bool TryPeekChar(out char character)
        {
            return TryPeekChar(0, out character);
        }

        private bool TryPeekChar(int position, out char character)
        {
            character = default;

            int result = _buffer.Peek(position);
            if (result < 0)
                return false;

            character = (char)result;
            return true;
        }

        private char ReadChar()
        {
            int result = _buffer.Read();
            if (result < 0)
                throw CreateException("Could not read character.");

            if (result == '\n')
            {
                Line++;
                Column = 0;
            }

            if (result == '\t')
                Column += 3;

            Column++;
            Position++;

            return (char)result;
        }

        private Exception CreateException(string message, string? expected = null)
        {
            message = $"{message} (Line {Line}, Column {Column})";

            if (!string.IsNullOrEmpty(expected))
                message = $"{message} (Expected \"{expected}\")";

            throw new InvalidOperationException(message);
        }
    }
}
