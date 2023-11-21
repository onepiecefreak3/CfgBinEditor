using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.Messages
{
    public class ValueSettingsChangedMessage
    {
        public object Sender { get; }

        public string GameName { get; }
        public string EntryName { get; }

        public ValueSettingsChangedMessage(object sender, string game, string entryName)
        {
            Sender = sender;

            GameName = game;
            EntryName = entryName;
        }
    }
}
