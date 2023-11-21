using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Business.CfgBinValueSettingsManagement
{
    internal class ValueSettingsWriter : IValueSettingsWriter
    {
        private readonly CfgBinValueSettingsManagementConfiguration _config;
        private readonly ICfgBinValueSettingsWhitespaceNormalizer _normalizer;
        private readonly ICfgBinValueSettingsComposer _composer;
        private readonly ICfgBinValueSettingsSyntaxFactory _syntaxFactory;

        public ValueSettingsWriter(CfgBinValueSettingsManagementConfiguration config,
            ICfgBinValueSettingsWhitespaceNormalizer normalizer, ICfgBinValueSettingsComposer composer,
            ICfgBinValueSettingsSyntaxFactory syntaxFactory)
        {
            _config = config;
            _normalizer = normalizer;
            _composer = composer;
            _syntaxFactory = syntaxFactory;
        }

        public void Write(IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> settings)
        {
            ConfigUnitSyntax configUnit = CreateConfigUnit(settings);

            _normalizer.NormalizeConfigUnit(configUnit);
            string configText = _composer.ComposeConfigUnit(configUnit);

            string baseDir = Path.GetDirectoryName(Environment.ProcessPath)!;
            string settingsPath = Path.Combine(baseDir, _config.ValueSettingsPath);

            File.WriteAllText(settingsPath, configText);
        }

        private ConfigUnitSyntax CreateConfigUnit(IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> settings)
        {
            var gameConfigs = new List<GameConfigSyntax>();
            foreach (var gameConfig in settings)
                gameConfigs.Add(CreateGameConfig(gameConfig.Key, gameConfig.Value));

            return new ConfigUnitSyntax(gameConfigs);
        }

        private GameConfigSyntax CreateGameConfig(string name, IDictionary<string, IList<ValueSettingEntry>> gameSettings)
        {
            SyntaxToken identifier = _syntaxFactory.Identifier(name);
            SyntaxToken bracketOpen = _syntaxFactory.Token(SyntaxTokenKind.BracketOpen);
            SyntaxToken bracketClose = _syntaxFactory.Token(SyntaxTokenKind.BracketClose);

            var entryConfigs = new List<EntryConfigSyntax>();
            foreach (var entryConfig in gameSettings)
                entryConfigs.Add(CreateEntryConfig(entryConfig.Key, entryConfig.Value));

            return new GameConfigSyntax(identifier, bracketOpen, entryConfigs, bracketClose);
        }

        private EntryConfigSyntax CreateEntryConfig(string name, IList<ValueSettingEntry> entrySettings)
        {
            SyntaxToken identifier = _syntaxFactory.Identifier(name);
            SyntaxToken parenOpen = _syntaxFactory.Token(SyntaxTokenKind.ParenOpen);
            SyntaxToken parenClose = _syntaxFactory.Token(SyntaxTokenKind.ParenClose);

            var valueSettings = new List<EntryConfigSettingSyntax>();
            foreach (ValueSettingEntry entry in entrySettings)
                valueSettings.Add(CreateEntryConfigSetting(entry));

            return new EntryConfigSyntax(identifier, parenOpen, valueSettings, parenClose);
        }

        private EntryConfigSettingSyntax CreateEntryConfigSetting(ValueSettingEntry entry)
        {
            SyntaxToken name = _syntaxFactory.Identifier(entry.Name);
            SyntaxToken pipe = _syntaxFactory.Token(SyntaxTokenKind.Pipe);
            SyntaxToken isHex = entry.IsHex ?
                _syntaxFactory.Token(SyntaxTokenKind.TrueKeyword) :
                _syntaxFactory.Token(SyntaxTokenKind.FalseKeyword);

            return new EntryConfigSettingSyntax(name, pipe, isHex);
        }
    }
}
