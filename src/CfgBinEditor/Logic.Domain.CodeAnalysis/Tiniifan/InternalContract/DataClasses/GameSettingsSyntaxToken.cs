using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses
{
    public struct GameSettingsSyntaxToken
    {
        public SyntaxTokenKind Kind { get; }
        public string Text { get; }

        public int Position { get; }
        public int Line { get; }
        public int Column { get; }

        public GameSettingsSyntaxToken(SyntaxTokenKind kind, int position, int line, int column, string? text = null)
        {
            Text = text ?? string.Empty;
            Kind = kind;
            Position = position;
            Line = line;
            Column = column;
        }
    }
}
