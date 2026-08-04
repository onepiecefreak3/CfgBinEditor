namespace Logic.Domain.Level5Management.Contract.DataClasses;

public class T2bEntry
{
    public required string Name { get; set; }
    public required T2bEntryValue[] Values { get; set; }
}