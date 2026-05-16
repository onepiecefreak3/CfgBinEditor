using System.Diagnostics;

namespace Logic.Domain.Level5Management.Contract.DataClasses
{
    [DebuggerDisplay("Type {Name}")]
    public class RdbnTypeDeclaration
    {
        public string Name { get; set; }
        public uint UnkHash { get; set; }
        public RdbnFieldDeclaration[] Fields { get; set; }

        public override int GetHashCode()
        {
            int nameHash = Name.GetHashCode();

            foreach (RdbnFieldDeclaration field in Fields)
                nameHash = HashCode.Combine(nameHash, field.Name);

            return nameHash;
        }
    }
}
