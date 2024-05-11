using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public class GameConfigSyntax : SyntaxNode
    {
        public SyntaxToken Identifier { get; private set; }
        public SyntaxToken BracketOpen { get; private set; }
        public IList<EntryConfigSyntax> EntryConfigs { get; private set; }
        public SyntaxToken BracketClose { get; private set; }

        public override SyntaxLocation Location => Identifier.Location;
        public override SyntaxSpan Span => new(Identifier.FullSpan.Position, BracketClose.FullSpan.EndPosition);

        public GameConfigSyntax(SyntaxToken identifier, SyntaxToken bracketOpen, IList<EntryConfigSyntax>? entryConfigs, SyntaxToken bracketClose)
        {
            identifier.Parent = this;
            bracketOpen.Parent = this;
            bracketClose.Parent = this;
            if (entryConfigs != null)
                foreach (EntryConfigSyntax entryConfig in entryConfigs)
                    entryConfig.Parent = this;

            Identifier = identifier;
            BracketOpen = bracketOpen;
            BracketClose = bracketClose;
            EntryConfigs = entryConfigs ?? Array.Empty<EntryConfigSyntax>();

            Root.Update();
        }

        public void SetIdentifier(SyntaxToken identifier, bool updatePositions = true)
        {
            identifier.Parent = this;

            Identifier = identifier;

            if (updatePositions)
                Root.Update();
        }

        public void SetBracketOpen(SyntaxToken bracketOpen, bool updatePositions = true)
        {
            bracketOpen.Parent = this;

            BracketOpen = bracketOpen;

            if (updatePositions)
                Root.Update();
        }

        public void SetBracketClose(SyntaxToken bracketClose, bool updatePositions = true)
        {
            bracketClose.Parent = this;

            BracketClose = bracketClose;

            if (updatePositions)
                Root.Update();
        }

        internal override int UpdatePosition(int position, ref int line, ref int column)
        {
            SyntaxToken identifier = Identifier;
            SyntaxToken bracketOpen = BracketOpen;
            SyntaxToken bracketClose = BracketClose;

            position = identifier.UpdatePosition(position, ref line, ref column);
            position = bracketOpen.UpdatePosition(position, ref line, ref column);
            foreach (EntryConfigSyntax entryConfig in EntryConfigs)
                position = entryConfig.UpdatePosition(position, ref line, ref column);
            position = bracketClose.UpdatePosition(position, ref line, ref column);

            Identifier = identifier;
            BracketOpen = bracketOpen;
            BracketClose = bracketClose;

            return position;
        }
    }
}
