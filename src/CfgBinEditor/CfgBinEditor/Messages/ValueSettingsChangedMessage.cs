using ImGui.Forms.Localization;

namespace CfgBinEditor.Messages
{
    public class ValueSettingsChangedMessage
    {
        public object Sender { get; }

        public LocalizedString GameName { get; }
        public string EntryName { get; }

        public ValueSettingsChangedMessage(object sender, LocalizedString game, string entryName)
        {
            Sender = sender;

            GameName = game;
            EntryName = entryName;
        }
    }
}