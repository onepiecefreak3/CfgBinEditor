using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses
{
    public class EntryConfigSettingSyntax : SyntaxNode
    {
        public SyntaxToken[] Value1 { get; private set; }
        public SyntaxToken Pipe { get; private set; }
        public SyntaxToken[] Value2 { get; private set; }

        public override SyntaxLocation Location => Value1.Length <= 0 ? Pipe.Location : Value1[0].Location;
        public override SyntaxSpan Span => new(Value1.Length <= 0 ? Pipe.FullSpan.Position : Value1[0].FullSpan.Position,
            Value2.Length <= 0 ? Pipe.FullSpan.EndPosition : Value2[^1].FullSpan.EndPosition);

        public EntryConfigSettingSyntax(SyntaxToken[] value1, SyntaxToken pipe, SyntaxToken[] value2)
        {
            for (var i = 0; i < value1.Length; i++)
            {
                SyntaxToken value = value1[i];
                value.Parent = this;
                value1[i] = value;
            }
            pipe.Parent = this;
            for (var i = 0; i < value2.Length; i++)
            {
                SyntaxToken value = value2[i];
                value.Parent = this;
                value2[i] = value;
            }

            Value1 = value1;
            Pipe = pipe;
            Value2 = value2;

            Root.Update();
        }

        public void SetValue1(SyntaxToken[] value1, bool updatePositions = true)
        {
            for (var i = 0; i < value1.Length; i++)
            {
                SyntaxToken value = value1[i];
                value.Parent = this;
                value1[i] = value;
            }

            Value1 = value1;

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

        public void SetValue2(SyntaxToken[] value2, bool updatePositions = true)
        {
            for (var i = 0; i < value2.Length; i++)
            {
                SyntaxToken value = value2[i];
                value.Parent = this;
                value2[i] = value;
            }

            Value2 = value2;

            if (updatePositions)
                Root.Update();
        }

        internal override int UpdatePosition(int position, ref int line, ref int column)
        {
            SyntaxToken[] value1 = Value1;
            SyntaxToken pipe = Pipe;
            SyntaxToken[] value2 = Value2;

            for (var i = 0; i < value1.Length; i++)
            {
                SyntaxToken value = value1[i];
                position = value.UpdatePosition(position, ref line, ref column);
                value1[i] = value;
            }
            position = pipe.UpdatePosition(position, ref line, ref column);
            for (var i = 0; i < value2.Length; i++)
            {
                SyntaxToken value = value2[i];
                position = value.UpdatePosition(position, ref line, ref column);
                value2[i] = value;
            }

            Value1 = value1;
            Pipe = pipe;
            Value2 = value2;

            return position;
        }
    }
}
