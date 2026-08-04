using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan;

public interface IGameSettingsWhitespaceNormalizer
{
    void NormalizeConfigUnit(ConfigUnitSyntax configUnit);
}