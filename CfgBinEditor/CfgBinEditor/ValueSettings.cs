using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.InternalContract;
using CfgBinEditor.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;

namespace CfgBinEditor
{
    internal class ValueSettings : IValueSettings
    {
        private readonly ICfgBinValueSettingsParser _settingsParser;
        private readonly IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> _settings;

        public bool HasError => ReadError != null;
        public Exception ReadError { get; private set; }

        public ValueSettings(CfgBinEditorConfiguration config, ICfgBinValueSettingsParser settingsParser)
        {
            _settingsParser = settingsParser;
            _settings = new Dictionary<string, IDictionary<string, IList<ValueSettingEntry>>>();

            InitializeValueNames(config.ValueSettingsPath);
        }

        public ValueSettingEntry GetEntrySettings(string game, string entryName, int index)
        {
            if (!_settings.TryGetValue(game, out IDictionary<string, IList<ValueSettingEntry>> entries))
                return ValueSettingEntry.Empty;

            if (!entries.TryGetValue(entryName, out IList<ValueSettingEntry> settings))
                return ValueSettingEntry.Empty;

            if (index < 0 || index >= settings.Count)
                return ValueSettingEntry.Empty;

            return settings[index];
        }

        private void InitializeValueNames(string settingsPath)
        {
            if (!TryParseSettings(settingsPath, out ConfigUnitSyntax configUnit))
                return;

            foreach (GameConfigSyntax gameConfig in configUnit.GameConfigs)
            {
                var entries = new Dictionary<string, IList<ValueSettingEntry>>();
                foreach (EntryConfigSyntax entryConfig in gameConfig.EntryConfigs)
                {
                    var configs = new List<ValueSettingEntry>();
                    foreach (EntryConfigSettingSyntax settings in entryConfig.Settings)
                    {
                        configs.Add(new ValueSettingEntry
                        {
                            Name = settings.Name.Text,
                            IsHex = settings.IsHex.RawKind == (int)SyntaxTokenKind.TrueKeyword
                        });
                    }

                    entries[entryConfig.Identifier.Text] = configs;
                }

                _settings[gameConfig.Identifier.Text] = entries;
            }
        }

        private bool TryParseSettings(string settingsPath, out ConfigUnitSyntax configUnit)
        {
            configUnit = null;

            if (!File.Exists(settingsPath))
            {
                ReadError = new FileNotFoundException($"File {settingsPath} not found.", settingsPath);
                return false;
            }

            try
            {
                string settingsText = File.ReadAllText(settingsPath);
                configUnit = _settingsParser.Parse(settingsText);
            }
            catch (Exception e)
            {
                ReadError = e;
                return false;
            }

            return true;
        }
    }
}
