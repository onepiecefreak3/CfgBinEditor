using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses
{
    public enum SyntaxTokenKind
    {
        BracketOpen,
        BracketClose,
        ParenOpen,
        ParenClose,
        Pipe,

        Identifier,
        Trivia,

        TrueKeyword,
        FalseKeyword,

        EndOfFile
    }
}
