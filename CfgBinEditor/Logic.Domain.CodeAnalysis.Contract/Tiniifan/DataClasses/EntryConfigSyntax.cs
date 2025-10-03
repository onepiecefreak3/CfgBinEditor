using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public class EntryConfigSyntax : SyntaxNode
    {
        public SyntaxToken[] Name { get; private set; }
        public SyntaxToken ParenOpen { get; private set; }
        public IList<EntryConfigSettingSyntax> Settings { get; private set; }
        public SyntaxToken ParenClose { get; private set; }

        public override SyntaxLocation Location => Name.Length <= 0 ? ParenOpen.FullLocation : Name[0].FullLocation;
        public override SyntaxSpan Span => new(Name.Length <= 0 ? ParenOpen.FullSpan.Position : Name[0].FullSpan.Position, ParenClose.FullSpan.EndPosition);

        public EntryConfigSyntax(SyntaxToken[] name, SyntaxToken parenOpen, IList<EntryConfigSettingSyntax>? settings, SyntaxToken parenClose)
        {
            for (var i = 0; i < name.Length; i++)
            {
                SyntaxToken value = name[i];
                value.Parent = this;
                name[i] = value;
            }
            parenOpen.Parent = this;
            parenClose.Parent = this;
            if (settings != null)
                foreach (EntryConfigSettingSyntax setting in settings)
                    setting.Parent = this;

            Name = name;
            ParenOpen = parenOpen;
            ParenClose = parenClose;
            Settings = settings ?? Array.Empty<EntryConfigSettingSyntax>();

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
            SyntaxToken[] name = Name;
            SyntaxToken parenOpen = ParenOpen;
            SyntaxToken parenClose = ParenClose;

            for (var i = 0; i < name.Length; i++)
            {
                SyntaxToken value = name[i];
                position = value.UpdatePosition(position, ref line, ref column);
                name[i] = value;
            }
            position = parenOpen.UpdatePosition(position, ref line, ref column);
            foreach (EntryConfigSettingSyntax setting in Settings)
                position = setting.UpdatePosition(position, ref line, ref column);
            position = parenClose.UpdatePosition(position, ref line, ref column);

            Name = name;
            ParenOpen = parenOpen;
            ParenClose = parenClose;

            return position;
        }
    }
}
