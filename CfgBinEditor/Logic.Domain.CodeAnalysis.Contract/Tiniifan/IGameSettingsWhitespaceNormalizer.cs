using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan
{
    [MapException(typeof(GameSettingsWhitespaceNormalizerException))]
    public interface IGameSettingsWhitespaceNormalizer
    {
        void NormalizeConfigUnit(ConfigUnitSyntax configUnit);
    }
}
