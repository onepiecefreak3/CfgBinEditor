using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.DataClasses
{
    public class ConfigUnitSyntax : SyntaxNode
    {
        public IList<GameConfigSyntax> GameConfigs { get; private set; }

        public override SyntaxLocation Location => GameConfigs.Count <= 0 ? new(1, 1) : GameConfigs[0].Location;
        public override SyntaxSpan Span => new(GameConfigs.Count <= 0 ? 0 : GameConfigs[0].Span.Position,
            GameConfigs.Count <= 0 ? 0 : GameConfigs[^1].Span.EndPosition);

        public ConfigUnitSyntax(IList<GameConfigSyntax>? gameConfigs)
        {
            if (gameConfigs != null)
                foreach (GameConfigSyntax gameConfig in gameConfigs)
                    gameConfig.Parent = this;

            GameConfigs = gameConfigs ?? Array.Empty<GameConfigSyntax>();

            Root.Update();
        }

        internal override int UpdatePosition(int position, ref int line, ref int column)
        {
            foreach (GameConfigSyntax gameConfig in GameConfigs)
                position += gameConfig.UpdatePosition(position, ref line, ref column);

            return position;
        }
    }
}
