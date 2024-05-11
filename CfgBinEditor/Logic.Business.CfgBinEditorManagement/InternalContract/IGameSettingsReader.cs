using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinEditorManagement.InternalContract.Exceptions;

namespace Logic.Business.CfgBinEditorManagement.InternalContract
{
    [MapException(typeof(GameSettingsReaderException))]
    public interface IGameSettingsReader<TEntry>
    {
        IDictionary<string, IDictionary<string, IList<TEntry>>> Read();
    }
}
