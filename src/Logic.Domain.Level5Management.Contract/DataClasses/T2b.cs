namespace Logic.Domain.Level5Management.Contract.DataClasses;

public class T2b
{
    public required T2bEntry[] Entries { get; set; }
    public StringEncoding Encoding { get; set; }
    public ValueLength ValueLength { get; set; }
    public HashType HashType { get; set; }
}