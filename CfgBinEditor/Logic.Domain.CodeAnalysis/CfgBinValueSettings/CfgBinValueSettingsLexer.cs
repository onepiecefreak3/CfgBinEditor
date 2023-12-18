using Logic.Domain.CodeAnalysis.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsLexer : ILexer<CfgBinValueSettingsSyntaxToken>
    {
        private readonly StringBuilder _sb;
        private readonly IBuffer<int> _buffer;

        public bool IsEndOfInput => _buffer.IsEndOfInput;

        private int Line { get; set; } = 1;
        private int Column { get; set; } = 1;
        private int Position { get; set; }

        public CfgBinValueSettingsLexer(IBuffer<int> buffer)
        {
            _sb = new StringBuilder();
            _buffer = buffer;
        }

        public CfgBinValueSettingsSyntaxToken Read()
        {
            if (!TryPeekChar(out char character))
                return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.EndOfFile, Position, Line, Column);

            switch (character)
            {
                case '[':
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.BracketOpen, Position, Line, Column, $"{ReadChar()}");
                case ']':
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.BracketClose, Position, Line, Column, $"{ReadChar()}");
                case '(':
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.ParenOpen, Position, Line, Column, $"{ReadChar()}");
                case ')':
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.ParenClose, Position, Line, Column, $"{ReadChar()}");
                case '|':
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.Pipe, Position, Line, Column, $"{ReadChar()}");

                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return ReadTriviaAndComments();

                default:
                    return ReadIdentifierOrKeyword();
            }
        }

        private CfgBinValueSettingsSyntaxToken ReadIdentifierOrKeyword()
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
            switch (finalValue)
            {
                case "True":
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.TrueKeyword, position, line, column, finalValue);

                case "False":
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.FalseKeyword, position, line, column, finalValue);

                default:
                    return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.Identifier, position, line, column, finalValue);
            }
        }

        private CfgBinValueSettingsSyntaxToken ReadTriviaAndComments()
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

            return new CfgBinValueSettingsSyntaxToken(SyntaxTokenKind.Trivia, position, line, column, _sb.ToString());
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
