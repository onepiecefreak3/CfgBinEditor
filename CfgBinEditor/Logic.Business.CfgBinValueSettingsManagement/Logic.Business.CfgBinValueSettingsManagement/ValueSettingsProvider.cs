using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;

namespace Logic.Business.CfgBinValueSettingsManagement
{
    internal class ValueSettingsProvider : IValueSettingsProvider
    {
        private readonly IValueSettingsReader _reader;
        private readonly IValueSettingsWriter _writer;
        
        private IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> _settings;
        private Exception? _readError;

        public ValueSettingsProvider(IValueSettingsReader reader, IValueSettingsWriter writer)
        {
            _reader = reader;
            _writer = writer;

            TryReadSettings();
        }

        public bool TryGetError(out Exception? error)
        {
            error = _readError;
            return _readError != null;
        }

        public ValueSettingEntry GetEntrySettings(string game, string entryName, int index)
        {
            if (!_settings.TryGetValue(game, out IDictionary<string, IList<ValueSettingEntry>>? entries))
                return ValueSettingEntry.Empty;

            if (!entries.TryGetValue(entryName, out IList<ValueSettingEntry>? settings))
                return ValueSettingEntry.Empty;

            if (index < 0 || index >= settings.Count)
                return ValueSettingEntry.Empty;

            return settings[index];
        }

        public void SetEntrySettings(string game, string entryName, int index, ValueSettingEntry entry)
        {
            if (!_settings.TryGetValue(game, out IDictionary<string, IList<ValueSettingEntry>>? entries))
                _settings[game] = entries = new Dictionary<string, IList<ValueSettingEntry>>();

            if (!entries.TryGetValue(entryName, out IList<ValueSettingEntry>? settings))
                entries[entryName] = settings = new List<ValueSettingEntry>();

            for (int i = settings.Count; i < index; i++)
                settings.Add(new ValueSettingEntry { Name = $"Unk{i}", IsHex = false });

            if (string.IsNullOrEmpty(entry.Name))
                entry.Name = $"Unk{index}";

            if (settings.Count > index)
                settings[index] = entry;
            else
                settings.Add(entry);
        }

        public void Persist()
        {
            _writer.Write(_settings);
        }

        private void TryReadSettings()
        {
            try
            {
                _settings = _reader.Read();
            }
            catch (Exception e)
            {
                _readError = e;
            }
        }
    }
}
