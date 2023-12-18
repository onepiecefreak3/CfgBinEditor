using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.CfgBinValueSettings.InternalContract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings;
using Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.CfgBinValueSettings
{
    internal class CfgBinValueSettingsWhitespaceNormalizer : ICfgBinValueSettingsWhitespaceNormalizer
    {
        public void NormalizeConfigUnit(ConfigUnitSyntax configUnit)
        {
            var ctx = new WhitespaceNormalizeContext();

            NormalizeGameConfigs(configUnit.GameConfigs, ctx);
        }

        private void NormalizeGameConfigs(IList<GameConfigSyntax> gameConfigs, WhitespaceNormalizeContext ctx)
        {
            ctx.ShouldLineBreak = true;
            for (var i = 0; i < gameConfigs.Count - 1; i++)
                NormalizeGameConfig(gameConfigs[i], ctx);

            ctx.ShouldLineBreak = false;
            NormalizeGameConfig(gameConfigs[^1], ctx);
        }

        private void NormalizeGameConfig(GameConfigSyntax gameConfig, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken identifier = gameConfig.Identifier.WithLeadingTrivia(null).WithTrailingTrivia(" ");
            SyntaxToken bracketOpen = gameConfig.BracketOpen.WithLeadingTrivia(null).WithTrailingTrivia("\r\n");
            SyntaxToken bracketClose = gameConfig.BracketClose.WithLeadingTrivia("\r\n").WithTrailingTrivia(null);

            if (ctx.ShouldLineBreak)
                bracketClose = bracketClose.WithTrailingTrivia("\r\n\r\n");

            ctx.Indent++;
            ctx.ShouldIndent = true;
            NormalizeEntryConfigs(gameConfig.EntryConfigs, ctx);

            gameConfig.SetIdentifier(identifier, false);
            gameConfig.SetBracketOpen(bracketOpen, false);
            gameConfig.SetBracketClose(bracketClose, false);
        }

        private void NormalizeEntryConfigs(IList<EntryConfigSyntax> entryConfigs, WhitespaceNormalizeContext ctx)
        {
            ctx.ShouldLineBreak = true;
            for (var i = 0; i < entryConfigs.Count - 1; i++)
                NormalizeEntryConfig(entryConfigs[i], ctx);

            ctx.ShouldLineBreak = false;
            NormalizeEntryConfig(entryConfigs[^1], ctx);
        }

        private void NormalizeEntryConfig(EntryConfigSyntax entryConfig, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken identifier = entryConfig.Identifier.WithLeadingTrivia(null).WithTrailingTrivia(" ");
            SyntaxToken parenOpen = entryConfig.ParenOpen.WithLeadingTrivia(null).WithTrailingTrivia("\r\n");
            SyntaxToken parenClose = entryConfig.ParenClose.WithNoTrivia();

            if (ctx is { Indent: > 0, ShouldIndent: true })
            {
                identifier = identifier.WithLeadingTrivia(new string('\t', ctx.Indent));
                parenClose = parenClose.WithLeadingTrivia(new string('\t', ctx.Indent));
            }

            if (ctx.ShouldLineBreak)
                parenClose = parenClose.WithTrailingTrivia("\r\n\r\n");

            ctx.Indent++;
            ctx.ShouldIndent = true;
            NormalizeEntryConfigSettings(entryConfig.Settings, ctx);

            entryConfig.SetIdentifier(identifier, false);
            entryConfig.SetParenOpen(parenOpen, false);
            entryConfig.SetParenClose(parenClose, false);
        }

        private void NormalizeEntryConfigSettings(IList<EntryConfigSettingSyntax> settings,
            WhitespaceNormalizeContext ctx)
        {
            ctx.ShouldLineBreak = true;
            foreach (EntryConfigSettingSyntax configSetting in settings)
                NormalizeEntryConfigSetting(configSetting, ctx);
        }

        private void NormalizeEntryConfigSetting(EntryConfigSettingSyntax configSetting, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken name = configSetting.Name.WithNoTrivia();
            SyntaxToken pipe = configSetting.Pipe.WithNoTrivia();
            SyntaxToken isHex = configSetting.IsHex.WithNoTrivia();

            if (ctx is { Indent: > 0, ShouldIndent: true })
                name = name.WithLeadingTrivia(new string('\t', ctx.Indent));

            if (ctx.ShouldLineBreak)
                isHex = isHex.WithTrailingTrivia("\r\n");

            configSetting.SetName(name, false);
            configSetting.SetPipe(pipe, false);
            configSetting.SetIsHex(isHex, false);
        }
    }
}
