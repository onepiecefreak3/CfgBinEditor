using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan;

public interface IGameSettingsComposer
{
    string ComposeConfigUnit(ConfigUnitSyntax configUnit);
}