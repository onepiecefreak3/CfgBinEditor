using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsComposer : ICfgBinValueSettingsComposer
    {
        public string ComposeConfigUnit(ConfigUnitSyntax configUnit)
        {
            var sb = new StringBuilder();

            foreach (var gameConfig in configUnit.GameConfigs)
                ComposeGameConfig(gameConfig, sb);

            return sb.ToString();
        }

        private void ComposeGameConfig(GameConfigSyntax gameConfig, StringBuilder sb)
        {
            ComposeSyntaxToken(gameConfig.Identifier, sb);
            ComposeSyntaxToken(gameConfig.BracketOpen, sb);

            foreach (var entryConfig in gameConfig.EntryConfigs)
                ComposeEntryConfig(entryConfig, sb);

            ComposeSyntaxToken(gameConfig.BracketClose, sb);
        }

        private void ComposeEntryConfig(EntryConfigSyntax entryConfig, StringBuilder sb)
        {
            ComposeSyntaxToken(entryConfig.Identifier, sb);
            ComposeSyntaxToken(entryConfig.ParenOpen, sb);

            foreach (var setting in entryConfig.Settings)
                ComposeEntryConfigSetting(setting, sb);

            ComposeSyntaxToken(entryConfig.ParenClose, sb);
        }

        private void ComposeEntryConfigSetting(EntryConfigSettingSyntax entryConfigSetting, StringBuilder sb)
        {
            ComposeSyntaxToken(entryConfigSetting.Name, sb);
            ComposeSyntaxToken(entryConfigSetting.Pipe, sb);
            ComposeSyntaxToken(entryConfigSetting.IsHex, sb);
        }

        private void ComposeSyntaxToken(SyntaxToken token, StringBuilder sb)
        {
            if (token.LeadingTrivia.HasValue)
                sb.Append(token.LeadingTrivia.Value.Text);

            sb.Append(token.Text);

            if (token.TrailingTrivia.HasValue)
                sb.Append(token.TrailingTrivia.Value.Text);
        }
    }
}
