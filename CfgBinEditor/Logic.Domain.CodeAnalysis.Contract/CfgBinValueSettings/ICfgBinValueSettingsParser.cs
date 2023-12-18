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
    [MapException(typeof(CfgBinValueSettingsParserException))]
    public interface ICfgBinValueSettingsParser
    {
        ConfigUnitSyntax Parse(string text);
    }
}
