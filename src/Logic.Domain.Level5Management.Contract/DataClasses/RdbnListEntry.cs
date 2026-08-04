using System.Diagnostics;

namespace Logic.Domain.Level5Management.Contract.DataClasses;

[DebuggerDisplay("Root {Name}")]
public class RdbnListEntry
{
    public required string Name { get; set; }
    public int TypeIndex { get; set; }
    public required object[][][] Values { get; set; }
}