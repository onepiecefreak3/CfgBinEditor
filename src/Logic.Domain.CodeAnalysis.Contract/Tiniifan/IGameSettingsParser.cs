using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan;

public interface IGameSettingsParser
{
    ConfigUnitSyntax Parse(string text);
}