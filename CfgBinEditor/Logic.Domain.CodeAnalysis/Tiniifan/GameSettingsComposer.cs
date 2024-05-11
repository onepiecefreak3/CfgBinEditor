using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using System.Text;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsComposer : IGameSettingsComposer
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
            ComposeSyntaxTokens(entryConfigSetting.Value1, sb);
            ComposeSyntaxToken(entryConfigSetting.Pipe, sb);
            ComposeSyntaxTokens(entryConfigSetting.Value2, sb);
        }

        private void ComposeSyntaxTokens(SyntaxToken[] tokens, StringBuilder sb)
        {
            foreach (SyntaxToken token in tokens)
                ComposeSyntaxToken(token, sb);
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
