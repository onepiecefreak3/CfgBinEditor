namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public enum SyntaxTokenKind
    {
        BracketOpen,
        BracketClose,
        ParenOpen,
        ParenClose,
        Pipe,

        NumericLiteral,

        Identifier,
        Trivia,

        TrueKeyword,
        FalseKeyword,

        EndOfFile
    }
}
