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
            throw new NotImplementedException();
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
