using System.Globalization;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan;
using Logic.Domain.CodeAnalysis.Contract.Tiniifan.DataClasses;

namespace Logic.Business.CfgBinEditorManagement
{
    // MyIDs.txt
    internal class EntryNamesReader : GameSettingsReader<EntryNameEntry>
    {
        public EntryNamesReader(CfgBinValueSettingsManagementConfiguration config, IGameSettingsParser parser)
            : base(config.EntryNamesPath, parser)
        {
        }

        protected override EntryNameEntry CreateEntry(EntryConfigSettingSyntax settings)
        {
            return new EntryNameEntry
            {
                Id = GetId(settings),
                Name = GetName(settings)
            };
        }

        private long GetId(EntryConfigSettingSyntax settings)
        {
            long id = 0;
            if (settings.Value1[0].RawKind == (int)SyntaxTokenKind.NumericLiteral)
            {
                string idText = settings.Value1[0].Text;
                bool isHex = idText.StartsWith("0x");

                if (isHex)
                    idText = idText[2..];

                if (!long.TryParse(idText, isHex ? NumberStyles.HexNumber : NumberStyles.None, CultureInfo.InvariantCulture, out id))
                    return 0;
            }

            return id;
        }

        private string GetName(EntryConfigSettingSyntax settings)
        {
            if (settings.Value2.Length <= 0)
                return string.Empty;

            return string.Join("", settings.Value2.Take(settings.Value2.Length - 1)) +
                   settings.Value2[^1].WithTrailingTrivia(null);
        }
    }
}
