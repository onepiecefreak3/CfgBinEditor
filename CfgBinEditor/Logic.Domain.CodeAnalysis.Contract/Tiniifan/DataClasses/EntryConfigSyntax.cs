using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public class EntryConfigSyntax : SyntaxNode
    {
        public SyntaxToken Identifier { get; private set; }
        public SyntaxToken ParenOpen { get; private set; }
        public IList<EntryConfigSettingSyntax> Settings { get; private set; }
        public SyntaxToken ParenClose { get; private set; }

        public override SyntaxLocation Location => Identifier.Location;
        public override SyntaxSpan Span => new(Identifier.FullSpan.Position, ParenClose.FullSpan.EndPosition);

        public EntryConfigSyntax(SyntaxToken identifier, SyntaxToken parenOpen, IList<EntryConfigSettingSyntax>? settings, SyntaxToken parenClose)
        {
            identifier.Parent = this;
            parenOpen.Parent = this;
            parenClose.Parent = this;
            if (settings != null)
                foreach (EntryConfigSettingSyntax setting in settings)
                    setting.Parent = this;

            Identifier = identifier;
            ParenOpen = parenOpen;
            ParenClose = parenClose;
            Settings = settings ?? Array.Empty<EntryConfigSettingSyntax>();

            Root.Update();
        }

        public void SetIdentifier(SyntaxToken identifier, bool updatePositions = true)
        {
            identifier.Parent = this;

            Identifier = identifier;

            if (updatePositions)
                Root.Update();
        }

        public void SetParenOpen(SyntaxToken parenOpen, bool updatePositions = true)
        {
            parenOpen.Parent = this;

            ParenOpen = parenOpen;

            if (updatePositions)
                Root.Update();
        }

        public void SetParenClose(SyntaxToken parenClose, bool updatePositions = true)
        {
            parenClose.Parent = this;

            ParenClose = parenClose;

            if (updatePositions)
                Root.Update();
        }

        internal override int UpdatePosition(int position, ref int line, ref int column)
        {
            SyntaxToken identifier = Identifier;
            SyntaxToken parenOpen = ParenOpen;
            SyntaxToken parenClose = ParenClose;

            position = identifier.UpdatePosition(position, ref line, ref column);
            position = parenOpen.UpdatePosition(position, ref line, ref column);
            foreach (EntryConfigSettingSyntax setting in Settings)
                position = setting.UpdatePosition(position, ref line, ref column);
            position = parenClose.UpdatePosition(position, ref line, ref column);

            Identifier = identifier;
            ParenOpen = parenOpen;
            ParenClose = parenClose;

            return position;
        }
    }
}
