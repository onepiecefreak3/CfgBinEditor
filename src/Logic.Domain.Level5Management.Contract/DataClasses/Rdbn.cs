namespace Logic.Domain.Level5Management.Contract.DataClasses;

public class Rdbn
{
    public required RdbnTypeDeclaration[] Types { get; set; }
    public required RdbnListEntry[] Lists { get; set; }
}