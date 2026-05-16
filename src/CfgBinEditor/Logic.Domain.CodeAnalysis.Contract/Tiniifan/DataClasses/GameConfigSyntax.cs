using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public class GameConfigSyntax : SyntaxNode
    {
        public SyntaxToken[] Name { get; private set; }
        public SyntaxToken BracketOpen { get; private set; }
        public IList<EntryConfigSyntax> EntryConfigs { get; private set; }
        public SyntaxToken BracketClose { get; private set; }

        public override SyntaxLocation Location => Name.Length <= 0 ? BracketOpen.FullLocation : Name[0].FullLocation;
        public override SyntaxSpan Span => new(Name.Length <= 0 ? BracketOpen.FullSpan.Position : Name[0].FullSpan.Position, BracketClose.FullSpan.EndPosition);

        public GameConfigSyntax(SyntaxToken[] name, SyntaxToken bracketOpen, IList<EntryConfigSyntax>? entryConfigs, SyntaxToken bracketClose)
        {
            for (var i = 0; i < name.Length; i++)
            {
                SyntaxToken value = name[i];
                value.Parent = this;
                name[i] = value;
            }
            bracketOpen.Parent = this;
            bracketClose.Parent = this;
            if (entryConfigs != null)
                foreach (EntryConfigSyntax entryConfig in entryConfigs)
                    entryConfig.Parent = this;

            Name = name;
            BracketOpen = bracketOpen;
            BracketClose = bracketClose;
            EntryConfigs = entryConfigs ?? Array.Empty<EntryConfigSyntax>();

            Root.Update();
        }

        public void SetName(SyntaxToken[] name, bool updatePositions = true)
        {
            for (var i = 0; i < name.Length; i++)
            {
                SyntaxToken value = name[i];
                value.Parent = this;
                name[i] = value;
            }

            Name = name;

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
            SyntaxToken[] name = Name;
            SyntaxToken bracketOpen = BracketOpen;
            SyntaxToken bracketClose = BracketClose;

            for (var i = 0; i < name.Length; i++)
            {
                SyntaxToken value = name[i];
                position = value.UpdatePosition(position, ref line, ref column);
                name[i] = value;
            }
            position = bracketOpen.UpdatePosition(position, ref line, ref column);
            foreach (EntryConfigSyntax entryConfig in EntryConfigs)
                position = entryConfig.UpdatePosition(position, ref line, ref column);
            position = bracketClose.UpdatePosition(position, ref line, ref column);

            Name = name;
            BracketOpen = bracketOpen;
            BracketClose = bracketClose;

            return position;
        }
    }
}
