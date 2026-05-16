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
                Name = GetCompositeText(settings.Value1),
                IsHex = settings.Value2[0].RawKind == (int)SyntaxTokenKind.TrueKeyword
            };
        }
    }
}
