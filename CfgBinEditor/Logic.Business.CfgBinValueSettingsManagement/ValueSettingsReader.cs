using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;

namespace Logic.Business.CfgBinValueSettingsManagement
{
    internal class ValueSettingsReader : IValueSettingsReader
    {
        private readonly CfgBinValueSettingsManagementConfiguration _config;
        private readonly ICfgBinValueSettingsParser _parser;

        public ValueSettingsReader(CfgBinValueSettingsManagementConfiguration config, ICfgBinValueSettingsParser parser)
        {
            _config = config;
            _parser = parser;
        }

        public IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> Read()
        {
            ConfigUnitSyntax configUnit = ParseSettings(_config.ValueSettingsPath);
            return ProcessSettings(configUnit);
        }

        private IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> ProcessSettings(ConfigUnitSyntax configUnit)
        {
            var result = new Dictionary<string, IDictionary<string, IList<ValueSettingEntry>>>();

            foreach (GameConfigSyntax gameConfig in configUnit!.GameConfigs)
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

                result[gameConfig.Identifier.Text] = entries;
            }

            return result;
        }

        private ConfigUnitSyntax ParseSettings(string settingsPath)
        {
            string baseDir = Path.GetDirectoryName(Environment.ProcessPath)!;
            settingsPath = Path.Combine(baseDir, settingsPath);

            string settingsText = File.ReadAllText(settingsPath);
            return _parser.Parse(settingsText);
        }
    }
}
