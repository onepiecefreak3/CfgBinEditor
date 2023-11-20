using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings
{
    [MapException(typeof(CfgBinValueSettingsWhitespaceNormalizerException))]
    public interface ICfgBinValueSettingsWhitespaceNormalizer
    {
        void NormalizeConfigUnit(ConfigUnitSyntax configUnit);
    }
}
