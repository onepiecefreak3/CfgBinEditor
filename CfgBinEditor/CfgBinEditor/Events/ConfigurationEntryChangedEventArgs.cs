using System;
using Logic.Domain.Level5.Contract.DataClasses;

namespace CfgBinEditor.Events
{
    public class ConfigurationEntryChangedEventArgs : EventArgs
    {
        public ConfigurationEntry Entry { get; }

        public ConfigurationEntryChangedEventArgs(ConfigurationEntry entry)
        {
            Entry = entry;
        }
    }
}
