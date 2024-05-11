using Logic.Business.CfgBinEditorManagement.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Business.CfgBinEditorManagement
{
    internal abstract class GameSettingsReader<TEntry> : IGameSettingsReader<TEntry>
    {
        private readonly string _path;
        private readonly IGameSettingsParser _parser;

        public GameSettingsReader(string path, IGameSettingsParser parser)
        {
            _path = path;
            _parser = parser;
        }

        public IDictionary<string, IDictionary<string, IList<TEntry>>> Read()
        {
            ConfigUnitSyntax configUnit = ParseSettings(_path);
            return ProcessSettings(configUnit);
        }

        private IDictionary<string, IDictionary<string, IList<TEntry>>> ProcessSettings(ConfigUnitSyntax configUnit)
        {
            var result = new Dictionary<string, IDictionary<string, IList<TEntry>>>();

            foreach (GameConfigSyntax gameConfig in configUnit!.GameConfigs)
            {
                var entries = new Dictionary<string, IList<TEntry>>();
                foreach (EntryConfigSyntax entryConfig in gameConfig.EntryConfigs)
                {
                    var configs = new List<TEntry>();
                    foreach (EntryConfigSettingSyntax settings in entryConfig.Settings)
                    {
                        configs.Add(CreateEntry(settings));
                    }

                    entries[entryConfig.Identifier.Text] = configs;
                }

                result[gameConfig.Identifier.Text] = entries;
            }

            return result;
        }

        protected abstract TEntry CreateEntry(EntryConfigSettingSyntax settings);

        private ConfigUnitSyntax ParseSettings(string settingsPath)
        {
            string baseDir = Path.GetDirectoryName(Environment.ProcessPath)!;
            settingsPath = Path.Combine(baseDir, settingsPath);

            string settingsText = File.ReadAllText(settingsPath);
            return _parser.Parse(settingsText);
        }
    }
}
