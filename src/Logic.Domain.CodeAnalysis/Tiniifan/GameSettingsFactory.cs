using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Tiniifan.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Tiniifan;

internal class GameSettingsFactory : ITokenFactory<GameSettingsSyntaxToken>
{
    public ILexer<GameSettingsSyntaxToken> CreateLexer(string text)
    {
        IBuffer<int> buffer = new StringBuffer(text);
        return new GameSettingsLexer(buffer);
    }

    public IBuffer<GameSettingsSyntaxToken> CreateTokenBuffer(ILexer<GameSettingsSyntaxToken> lexer)
    {
        return new TokenBuffer<GameSettingsSyntaxToken>(lexer);
    }
}
