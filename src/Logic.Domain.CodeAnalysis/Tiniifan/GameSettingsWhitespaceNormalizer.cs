using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan
{
    internal class GameSettingsWhitespaceNormalizer : IGameSettingsWhitespaceNormalizer
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
            SyntaxToken namePart = gameConfig.Name[^1].WithTrailingTrivia(" ");
            gameConfig.Name[^1] = namePart;

            namePart = gameConfig.Name[0].WithLeadingTrivia(null);
            gameConfig.Name[0] = namePart;

            SyntaxToken bracketOpen = gameConfig.BracketOpen.WithLeadingTrivia(null).WithTrailingTrivia("\r\n");
            SyntaxToken bracketClose = gameConfig.BracketClose.WithLeadingTrivia("\r\n").WithTrailingTrivia(null);

            if (ctx.ShouldLineBreak)
                bracketClose = bracketClose.WithTrailingTrivia("\r\n\r\n");

            ctx.Indent++;
            ctx.ShouldIndent = true;
            NormalizeEntryConfigs(gameConfig.EntryConfigs, ctx);

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
            SyntaxToken namePart = entryConfig.Name[^1].WithTrailingTrivia(" ");
            entryConfig.Name[^1] = namePart;

            namePart = entryConfig.Name[0].WithLeadingTrivia(null);

            SyntaxToken parenOpen = entryConfig.ParenOpen.WithLeadingTrivia(null).WithTrailingTrivia("\r\n");
            SyntaxToken parenClose = entryConfig.ParenClose.WithNoTrivia();

            if (ctx is { Indent: > 0, ShouldIndent: true })
            {
                namePart = namePart.WithLeadingTrivia(new string('\t', ctx.Indent));
                parenClose = parenClose.WithLeadingTrivia(new string('\t', ctx.Indent));
            }

            if (ctx.ShouldLineBreak)
                parenClose = parenClose.WithTrailingTrivia("\r\n\r\n");

            ctx.Indent++;
            ctx.ShouldIndent = true;
            NormalizeEntryConfigSettings(entryConfig.Settings, ctx);

            entryConfig.Name[0] = namePart;

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
            SyntaxToken value1 = configSetting.Value1[0].WithLeadingTrivia(null);
            SyntaxToken pipe = configSetting.Pipe.WithNoTrivia();
            SyntaxToken value2 = configSetting.Value2[^1].WithTrailingTrivia(null);

            if (ctx is { Indent: > 0, ShouldIndent: true })
                value1 = value1.WithLeadingTrivia(new string('\t', ctx.Indent));

            if (ctx.ShouldLineBreak)
                value2 = value2.WithTrailingTrivia("\r\n");

            configSetting.Value1[0] = value1;
            configSetting.Value2[^1] = value2;

            configSetting.SetPipe(pipe, false);
        }
    }
}
