using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    internal class GameAddedMessage
    {
        public T2bForm Sender { get; }
        public string Game { get; }

        public GameAddedMessage(T2bForm sender, string game)
        {
            Sender = sender;
            Game = game;
        }
    }
}