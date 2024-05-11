using Logic.Business.CfgBinEditorManagement.Contract;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Business.CfgBinEditorManagement.InternalContract;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Business.CfgBinEditorManagement
{
    internal class EntryNamesProvider : IEntryNamesProvider
    {
        private readonly IGameSettingsReader<EntryNameEntry> _reader;

        private IDictionary<string, IDictionary<string, IDictionary<long, string>>> _names;
        private Exception? _readError;

        public EntryNamesProvider(IGameSettingsReader<EntryNameEntry> reader)
        {
            _reader = reader;

            TryReadNames();
        }

        public bool TryGetError(out Exception? error)
        {
            error = _readError;
            return _readError != null;
        }

        public bool TryGetName(string game, T2bEntry entry, ValueLength length, out string? name)
        {
            name = null;

            if (entry.Values.Length <= 0)
                return false;

            if (!_names.TryGetValue(game, out IDictionary<string, IDictionary<long, string>>? gameNames))
                return false;

            foreach (string key in gameNames.Keys)
            {
                switch (length)
                {
                    case ValueLength.Int:
                        if (gameNames[key].TryGetValue((int)entry.Values[0].Value, out name))
                            return true;

                        break;

                    case ValueLength.Long:
                        if (gameNames[key].TryGetValue((long)entry.Values[0].Value, out name))
                            return true;

                        break;
                }
            }

            return false;
        }

        private void TryReadNames()
        {
            try
            {
                _names = new Dictionary<string, IDictionary<string, IDictionary<long, string>>>();

                IDictionary<string, IDictionary<string, IList<EntryNameEntry>>> result = _reader.Read();
                foreach (string game in result.Keys)
                {
                    _names[game] = new Dictionary<string, IDictionary<long, string>>();
                    foreach (string sectionName in result[game].Keys)
                    {
                        _names[game][sectionName] = result[game][sectionName].ToDictionary(x => x.Id, y => y.Name);
                    }
                }
            }
            catch (Exception e)
            {
                _readError = e;
            }
        }
    }
}
