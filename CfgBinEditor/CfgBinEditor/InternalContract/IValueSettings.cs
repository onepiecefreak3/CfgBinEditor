using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.InternalContract.DataClasses;
using CfgBinEditor.InternalContract.Exceptions;
using CrossCutting.Core.Contract.Aspects;

namespace CfgBinEditor.InternalContract
{
    [MapException(typeof(ValueNameCacheException))]
    public interface IValueSettings
    {
        bool HasError { get; }
        Exception ReadError { get; }

        ValueSettingEntry GetEntrySettings(string game, string entryName, int index);
    }
}
