using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses
{
    public class EntryConfigSettingSyntax : SyntaxNode
    {
        public SyntaxToken Name { get; private set; }
        public SyntaxToken Pipe { get; private set; }
        public SyntaxToken IsHex { get; private set; }

        public override SyntaxLocation Location => Name.Location;
        public override SyntaxSpan Span => new(Name.FullSpan.Position, IsHex.FullSpan.EndPosition);

        public EntryConfigSettingSyntax(SyntaxToken name, SyntaxToken pipe, SyntaxToken isHex)
        {
            name.Parent = this;
            pipe.Parent = this;
            isHex.Parent = this;

            Name = name;
            Pipe = pipe;
            IsHex = isHex;

            Root.Update();
        }

        public void SetName(SyntaxToken name, bool updatePositions = true)
        {
            name.Parent = this;

            Name = name;

            if (updatePositions)
                Root.Update();
        }

        public void SetPipe(SyntaxToken pipe, bool updatePositions = true)
        {
            pipe.Parent = this;

            Pipe = pipe;

            if (updatePositions)
                Root.Update();
        }

        public void SetIsHex(SyntaxToken isHex, bool updatePositions = true)
        {
            isHex.Parent = this;

            IsHex = isHex;

            if (updatePositions)
                Root.Update();
        }

        internal override int UpdatePosition(int position, ref int line, ref int column)
        {
            SyntaxToken name = Name;
            SyntaxToken pipe = Pipe;
            SyntaxToken isHex = IsHex;

            position = name.UpdatePosition(position, ref line, ref column);
            position = pipe.UpdatePosition(position, ref line, ref column);
            position = isHex.UpdatePosition(position, ref line, ref column);

            Name = name;
            Pipe = pipe;
            IsHex = isHex;

            return position;
        }
    }
}
