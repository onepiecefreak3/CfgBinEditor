using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    internal class GameAddedMessage
    {
        public ConfigurationForm Sender { get; }
        public string Game { get; }

        public GameAddedMessage(ConfigurationForm sender, string game)
        {
            Sender = sender;
            Game = game;
        }
    }
}
