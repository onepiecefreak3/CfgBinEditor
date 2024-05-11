using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan;

namespace Logic.Business.CfgBinEditorManagement
{
    // MyTags.txt
    internal class ValueSettingsReader : GameSettingsReader<ValueSettingEntry>
    {
        public ValueSettingsReader(CfgBinValueSettingsManagementConfiguration config, IGameSettingsParser parser)
            : base(config.ValueSettingsPath, parser)
        {
        }

        protected override ValueSettingEntry CreateEntry(EntryConfigSettingSyntax settings)
        {
            return new ValueSettingEntry
            {
                Name = settings.Value1[0].Text,
                IsHex = settings.Value2[0].RawKind == (int)SyntaxTokenKind.TrueKeyword
            };
        }
    }
}
