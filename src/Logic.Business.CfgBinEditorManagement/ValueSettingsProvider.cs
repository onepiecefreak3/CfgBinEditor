using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Business.CfgBinEditorManagement.InternalContract;

namespace Logic.Business.CfgBinEditorManagement
{
    internal class ValueSettingsProvider : IValueSettingsProvider
    {
        private readonly IGameSettingsReader<ValueSettingEntry> _reader;
        private readonly IValueSettingsWriter _writer;

        private IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> _settings;
        private Exception? _readError;

        public ValueSettingsProvider(IGameSettingsReader<ValueSettingEntry> reader, IValueSettingsWriter writer)
        {
            _reader = reader;
            _writer = writer;

            _settings = new Dictionary<string, IDictionary<string, IList<ValueSettingEntry>>>();

            TryReadSettings();
        }

        public bool TryGetError(out Exception? error)
        {
            error = _readError;
            return _readError != null;
        }

        public string[] GetGames()
        {
            return _settings.Keys.ToArray();
        }

        public void AddGame(string game)
        {
            if (_settings.ContainsKey(game))
                return;

            _settings[game] = new Dictionary<string, IList<ValueSettingEntry>>();
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
