﻿namespace Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses
{
    internal struct WhitespaceNormalizeContext
    {
        public int Indent { get; set; }

        public bool ShouldIndent { get; set; }
        public bool ShouldLineBreak { get; set; }
        public bool IsFirstElement { get; set; }
    }
}
